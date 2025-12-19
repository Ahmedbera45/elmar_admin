'use client';

import { useState } from 'react';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { postAddAction } from '@/lib/api/generated';
import { useToast } from '@/components/ui/toast-context';

interface ProcessActionModalProps {
  isOpen: boolean;
  onClose: () => void;
  stepId: string;
  steps: { id: string; name: string }[];
  onSuccess: () => void;
}

export function ProcessActionModal({ isOpen, onClose, stepId, steps, onSuccess }: ProcessActionModalProps) {
  const [formData, setFormData] = useState({
    name: '',
    actionType: 1,
    targetStepId: ''
  });
  const { toast } = useToast();

  const handleSubmit = async () => {
    try {
      await postAddAction({
        stepId: stepId,
        name: formData.name,
        actionType: Number(formData.actionType),
        targetStepId: formData.targetStepId || null
      });
      toast("Action added successfully");
      onSuccess();
      onClose();
    } catch (e) {
      toast("Failed to add action", "error");
    }
  };

  const actionTypes = [
    { label: 'Approve', value: '1' },
    { label: 'Reject', value: '2' },
    { label: 'Request Change', value: '3' },
    { label: 'Cancel', value: '4' },
    { label: 'Delegate', value: '5' },
    { label: 'Submit', value: '6' },
    { label: 'Withdraw', value: '7' }
  ];

  const stepOptions = steps.map(s => ({ label: s.name, value: s.id }));

  return (
    <Dialog isOpen={isOpen} onClose={onClose} title="Add Action">
      <div className="space-y-4">
        <div>
          <Label>Name</Label>
          <Input
            value={formData.name}
            onChange={e => setFormData({...formData, name: e.target.value})}
            placeholder="e.g. Approve"
          />
        </div>
        <div>
          <Label>Type</Label>
          <Select
            value={formData.actionType.toString()}
            onChange={e => setFormData({...formData, actionType: Number(e.target.value)})}
            options={actionTypes}
          />
        </div>
        <div>
          <Label>Target Step</Label>
          <Select
            value={formData.targetStepId}
            onChange={e => setFormData({...formData, targetStepId: e.target.value})}
            options={[{ label: 'None', value: '' }, ...stepOptions]}
          />
        </div>
        <Button onClick={handleSubmit} className="w-full">Create Action</Button>
      </div>
    </Dialog>
  );
}
