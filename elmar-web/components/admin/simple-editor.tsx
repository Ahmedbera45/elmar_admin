'use client';

import { useRef, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Bold, Italic, Underline } from 'lucide-react';
import { Dialog } from '@/components/ui/dialog';

interface SimpleEditorProps {
  value: string;
  onChange: (val: string) => void;
  variables: { key: string; title: string }[];
}

export function SimpleEditor({ value, onChange, variables }: SimpleEditorProps) {
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);

  const insertAtCursor = (text: string) => {
    if (!textareaRef.current) return;
    const start = textareaRef.current.selectionStart;
    const end = textareaRef.current.selectionEnd;
    const currentVal = textareaRef.current.value;
    const newVal = currentVal.substring(0, start) + text + currentVal.substring(end);
    onChange(newVal);
  };

  const handleBold = () => insertAtCursor('<b></b>');
  const handleItalic = () => insertAtCursor('<i></i>');
  const handleUnderline = () => insertAtCursor('<u></u>');

  return (
    <div className="flex h-full gap-4">
      <div className="flex-1 flex flex-col gap-2">
         <div className="flex gap-2 p-2 bg-gray-100 rounded justify-between">
            <div className="flex gap-2">
                <Button variant="ghost" size="sm" onClick={handleBold}><Bold className="w-4 h-4"/></Button>
                <Button variant="ghost" size="sm" onClick={handleItalic}><Italic className="w-4 h-4"/></Button>
                <Button variant="ghost" size="sm" onClick={handleUnderline}><Underline className="w-4 h-4"/></Button>
                <span className="text-xs text-gray-500 self-center ml-2">HTML Supported</span>
            </div>
            <Button variant="outline" size="sm" onClick={() => setIsPreviewOpen(true)}>Preview</Button>
         </div>
         <textarea
            ref={textareaRef}
            className="flex-1 w-full p-4 border rounded font-mono text-sm resize-none"
            value={value}
            onChange={e => onChange(e.target.value)}
            placeholder="<html><body><h1>Title</h1>...</body></html>"
         />
      </div>

      <Dialog isOpen={isPreviewOpen} onClose={() => setIsPreviewOpen(false)} title="Preview">
        <div className="border p-4 min-h-[400px] prose max-w-none" dangerouslySetInnerHTML={{ __html: value }} />
      </Dialog>

      <div className="w-64 border-l pl-4 overflow-y-auto">
         <h4 className="font-bold mb-4">Variables</h4>
         <div className="space-y-2">
            {variables.map(v => (
                <div
                    key={v.key}
                    className="p-2 border rounded bg-white hover:bg-blue-50 cursor-pointer text-sm"
                    onClick={() => insertAtCursor(`{${v.key}}`)}
                >
                    {v.title}
                    <div className="text-xs text-gray-400">{v.key}</div>
                </div>
            ))}
         </div>
      </div>
    </div>
  );
}
