'use client';

import { useState } from 'react';
import { useGetProcessDefinition, postAddStep, postAddAction, postAddField } from '@/lib/api/generated';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/toast-context';
import { ProcessFieldModal } from '@/components/admin/process-field-modal';
import { ProcessActionModal } from '@/components/admin/process-action-modal';
import { StepSettingsModal } from '@/components/admin/step-settings-modal';
import { Settings } from 'lucide-react';

export default function ProcessDesignerPage({ params }: { params: { id: string } }) {
  const { data: process, isLoading, refetch } = useGetProcessDefinition(params.id);
  const [selectedStep, setSelectedStep] = useState<any>(null);
  const [isStepModalOpen, setIsStepModalOpen] = useState(false);
  const [isFieldModalOpen, setIsFieldModalOpen] = useState(false);
  const [isActionModalOpen, setIsActionModalOpen] = useState(false);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [newStepName, setNewStepName] = useState('');
  const { toast } = useToast();

  const handleAddStep = async () => {
    try {
      await postAddStep({ processId: params.id, name: newStepName, stepType: 2, orderIndex: (process?.steps?.length || 0) + 1 });
      toast("Step added");
      setIsStepModalOpen(false);
      refetch();
    } catch(e) { toast("Error", "error"); }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="flex h-[calc(100vh-100px)] gap-6">
      <div className="w-1/3 space-y-4 overflow-y-auto pr-2">
        <div className="flex justify-between items-center">
          <h2 className="text-xl font-bold">Steps</h2>
          <Button onClick={() => setIsStepModalOpen(true)}>+</Button>
        </div>
        {process?.steps.map((step: any) => (
          <Card
            key={step.id}
            className={`cursor-pointer relative ${selectedStep?.id === step.id ? 'border-blue-500' : ''}`}
            onClick={() => setSelectedStep(step)}
          >
            <CardHeader className="p-4 flex flex-row justify-between items-center">
              <CardTitle className="text-lg">{step.name}</CardTitle>
              <Button
                variant="ghost"
                size="icon"
                onClick={(e) => {
                  e.stopPropagation();
                  setSelectedStep(step);
                  setIsSettingsModalOpen(true);
                }}
              >
                <Settings className="h-4 w-4" />
              </Button>
            </CardHeader>
          </Card>
        ))}
      </div>

      <div className="flex-1 space-y-6 overflow-y-auto">
        {selectedStep ? (
          <>
            <Card>
              <CardHeader><CardTitle>Fields</CardTitle></CardHeader>
              <CardContent>
                <ul className="list-disc pl-5 mb-4">
                  {selectedStep.fields.map((f: any) => <li key={f.id}>{f.title} ({f.entryType})</li>)}
                </ul>
                <Button onClick={() => setIsFieldModalOpen(true)}>Add Field</Button>
              </CardContent>
            </Card>
            <Card>
              <CardHeader><CardTitle>Actions</CardTitle></CardHeader>
              <CardContent>
                <ul className="list-disc pl-5 mb-4">
                  {selectedStep.actions.map((a: any) => <li key={a.id}>{a.name} ({a.actionType})</li>)}
                </ul>
                <Button onClick={() => setIsActionModalOpen(true)}>Add Action</Button>
              </CardContent>
            </Card>
          </>
        ) : (
          <div className="flex items-center justify-center h-full text-gray-400">Select a step to edit</div>
        )}
      </div>

      <Dialog isOpen={isStepModalOpen} onClose={() => setIsStepModalOpen(false)} title="Add Step">
        <div className="space-y-4">
            <label>Name</label>
            <Input value={newStepName} onChange={e => setNewStepName(e.target.value)} />
            <Button onClick={handleAddStep} className="w-full">Add</Button>
        </div>
      </Dialog>

      <ProcessFieldModal
        isOpen={isFieldModalOpen}
        onClose={() => setIsFieldModalOpen(false)}
        stepId={selectedStep?.id}
        onSuccess={refetch}
      />
      <ProcessActionModal
        isOpen={isActionModalOpen}
        onClose={() => setIsActionModalOpen(false)}
        stepId={selectedStep?.id}
        steps={process?.steps || []}
        allFields={process?.steps.flatMap((s: any) => s.fields) || []}
        onSuccess={refetch}
      />
      <StepSettingsModal
        isOpen={isSettingsModalOpen}
        onClose={() => setIsSettingsModalOpen(false)}
        step={selectedStep}
        onSuccess={refetch}
      />
    </div>
  );
}
