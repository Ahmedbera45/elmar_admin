'use client';

import { useState, useEffect } from 'react';
import { useGetTemplate, useGetProcessDefinition, useGetProcesses, postSaveTemplate } from '@/lib/api/generated';
import { SimpleEditor } from '@/components/admin/simple-editor';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useToast } from '@/components/ui/toast-context';
import { Loader2 } from 'lucide-react';
import { useRouter } from 'next/navigation';

export default function TemplateDesignerPage({ params }: { params: { id: string } }) {
  const isNew = params.id === 'new';
  const { data: template, isLoading: loadingTemplate } = useGetTemplate(isNew ? '' : params.id);
  const { data: processes } = useGetProcesses();

  const [name, setName] = useState('');
  const [content, setContent] = useState('');
  const [processId, setProcessId] = useState('');
  const [saving, setSaving] = useState(false);
  const router = useRouter();
  const { toast } = useToast();

  // Fetch process definition when processId is selected to get fields
  const { data: processDef } = useGetProcessDefinition(processId || '');

  useEffect(() => {
    if (template) {
      setName(template.name);
      setContent(template.content);
      setProcessId(template.processId);
    }
  }, [template]);

  const handleSave = async () => {
    if (!processId || !name) {
      toast("Name and Process are required", "error");
      return;
    }
    setSaving(true);
    try {
      await postSaveTemplate({
        id: isNew ? '00000000-0000-0000-0000-000000000000' : params.id,
        processId,
        name,
        content
      });
      toast("Template saved");
      if (isNew) router.push('/dashboard/admin/templates');
    } catch (e) {
      toast("Error saving template", "error");
    } finally {
      setSaving(false);
    }
  };

  if (!isNew && loadingTemplate) return <div>Loading...</div>;

  const variables = processDef?.steps.flatMap((s: any) => s.fields) || [];

  return (
    <div className="flex flex-col h-[calc(100vh-100px)] space-y-4">
      <div className="flex justify-between items-center bg-white p-4 rounded shadow-sm">
         <div className="flex gap-4 items-center flex-1">
            <Input
                value={name}
                onChange={e => setName(e.target.value)}
                placeholder="Template Name"
                className="max-w-xs"
            />
            <Select value={processId} onValueChange={setProcessId} disabled={!isNew && !!processId}>
                <SelectTrigger className="w-[200px]">
                    <SelectValue placeholder="Select Process" />
                </SelectTrigger>
                <SelectContent>
                    {processes?.map((p: any) => (
                        <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>
                    ))}
                </SelectContent>
            </Select>
         </div>
         <div className="flex gap-2">
            <Button variant="outline" onClick={() => router.back()}>Cancel</Button>
            <Button onClick={handleSave} disabled={saving}>
                {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                Save
            </Button>
         </div>
      </div>

      <div className="flex-1 bg-white rounded shadow-sm p-4 overflow-hidden">
        <SimpleEditor
            value={content}
            onChange={setContent}
            variables={variables}
        />
      </div>
    </div>
  );
}
