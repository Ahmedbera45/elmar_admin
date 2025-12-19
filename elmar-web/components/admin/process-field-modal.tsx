'use client';

import { useState } from 'react';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { postAddField, useGetDatasets } from '@/lib/api/generated';
import { useToast } from '@/components/ui/toast-context';
import { useQuery } from '@tanstack/react-query';
import { customInstance } from '@/lib/api/custom-instance';

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
    entryType: 1, // Default to Text (1)
    isRequired: false,
    lookupSource: ''
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
        lookupSource: Number(formData.entryType) === 7 ? formData.lookupSource : null,
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
    { label: 'Text', value: '1' },
    { label: 'Number', value: '2' },
    { label: 'Date', value: '3' },
    { label: 'Select', value: '4' },
    { label: 'File', value: '5' },
    { label: 'Checkbox', value: '6' },
    { label: 'User Select', value: '7' }
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

        {Number(formData.entryType) === 7 && (
            <div>
                <Label>Lookup Role (Optional)</Label>
                <Input
                    value={formData.lookupSource}
                    onChange={e => setFormData({...formData, lookupSource: e.target.value})}
                    placeholder="e.g. Engineer"
                />
                <p className="text-xs text-gray-500 mt-1">If specified, only users with this role will be listed.</p>
            </div>
        )}

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
