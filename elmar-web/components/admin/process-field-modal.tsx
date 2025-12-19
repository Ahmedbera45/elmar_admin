'use client';

import { useState } from 'react';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { postAddField } from '@/lib/api/generated';
import { useToast } from '@/components/ui/toast-context';

interface ProcessFieldModalProps {
  isOpen: boolean;
  onClose: () => void;
  stepId: string;
  onSuccess: () => void;
}

export function ProcessFieldModal({ isOpen, onClose, stepId, onSuccess }: ProcessFieldModalProps) {
  const [formData, setFormData] = useState({
    title: '',
    key: '',
    entryType: 0,
    isRequired: false
  });
  const { toast } = useToast();

  const handleSubmit = async () => {
    try {
      await postAddField({
        stepId: stepId,
        key: formData.key,
        title: formData.title,
        entryType: Number(formData.entryType),
        isRequired: formData.isRequired,
        options: null
      });
      toast("Field added successfully");
      onSuccess();
      onClose();
    } catch (e) {
      toast("Failed to add field", "error");
    }
  };

  const entryTypes = [
    { label: 'Text', value: '0' },
    { label: 'Number', value: '1' },
    { label: 'Date', value: '2' },
    { label: 'Select', value: '3' },
    { label: 'File', value: '4' },
    { label: 'Checkbox', value: '5' }
  ];

  return (
    <Dialog isOpen={isOpen} onClose={onClose} title="Add Field">
      <div className="space-y-4">
        <div>
          <Label>Label</Label>
          <Input
            value={formData.title}
            onChange={e => setFormData({...formData, title: e.target.value})}
            placeholder="e.g. Birth Date"
          />
        </div>
        <div>
          <Label>Key (Database Name)</Label>
          <Input
            value={formData.key}
            onChange={e => setFormData({...formData, key: e.target.value})}
            placeholder="e.g. BirthDate"
          />
        </div>
        <div>
          <Label>Type</Label>
          <Select
            value={formData.entryType.toString()}
            onChange={e => setFormData({...formData, entryType: Number(e.target.value)})}
            options={entryTypes}
          />
        </div>
        <div className="flex items-center space-x-2">
          <input
            type="checkbox"
            checked={formData.isRequired}
            onChange={e => setFormData({...formData, isRequired: e.target.checked})}
            id="isRequired"
            className="h-4 w-4 rounded border-gray-300"
          />
          <Label htmlFor="isRequired">Required</Label>
        </div>
        <Button onClick={handleSubmit} className="w-full">Create Field</Button>
      </div>
    </Dialog>
  );
}
