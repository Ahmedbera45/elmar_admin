'use client';

import { useState, useEffect } from 'react';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { putUpdateStep, useGetRoles } from '@/lib/api/generated';
import { useToast } from '@/components/ui/toast-context';

interface StepSettingsModalProps {
  isOpen: boolean;
  onClose: () => void;
  step: any;
  onSuccess: () => void;
}

export function StepSettingsModal({ isOpen, onClose, step, onSuccess }: StepSettingsModalProps) {
  const [formData, setFormData] = useState({
    assignmentType: 1,
    assignedTo: ''
  });
  const { toast } = useToast();
  const { data: roles } = useGetRoles();

  useEffect(() => {
    if (step) {
      setFormData({
        assignmentType: step.assignmentType || 1,
        assignedTo: step.assignedTo || ''
      });
    }
  }, [step]);

  const handleSubmit = async () => {
    try {
      await putUpdateStep({
        stepId: step.id,
        assignmentType: Number(formData.assignmentType),
        assignedTo: formData.assignedTo
      });
      toast("Step updated successfully");
      onSuccess();
      onClose();
    } catch (e) {
      toast("Failed to update step", "error");
    }
  };

  const assignmentTypes = [
    { label: 'Role Based', value: '1' },
    { label: 'User Based', value: '2' },
    { label: 'Dynamic (From Field)', value: '3' }
  ];

  return (
    <Dialog isOpen={isOpen} onClose={onClose} title={`Settings: ${step?.name}`}>
      <div className="space-y-4">
        <div>
          <Label>Assignment Type</Label>
          <Select
            value={formData.assignmentType.toString()}
            onChange={e => setFormData({...formData, assignmentType: Number(e.target.value)})}
            options={assignmentTypes}
          />
        </div>

        {Number(formData.assignmentType) === 1 && (
            <div>
                <Label>Role</Label>
                <Select
                    value={formData.assignedTo}
                    onChange={e => setFormData({...formData, assignedTo: e.target.value})}
                    options={roles?.map((r: any) => ({ label: r.name, value: r.name })) || []}
                    placeholder="Select a role"
                />
            </div>
        )}

        {Number(formData.assignmentType) === 2 && (
            <div>
                <Label>User ID</Label>
                <Input
                    value={formData.assignedTo}
                    onChange={e => setFormData({...formData, assignedTo: e.target.value})}
                    placeholder="e.g. User GUID"
                />
            </div>
        )}

        {Number(formData.assignmentType) === 3 && (
            <div>
                <Label>Field Key (Source of User ID)</Label>
                <Input
                    value={formData.assignedTo}
                    onChange={e => setFormData({...formData, assignedTo: e.target.value})}
                    placeholder="e.g. ControllerUserId"
                />
                <p className="text-xs text-gray-500 mt-1">Enter the Key of the field that will contain the User ID.</p>
            </div>
        )}

        {/* Phase 13: Signing Step Config */}
        <div>
             <Label>Signature Document Template</Label>
             <Input
                placeholder="Template GUID (e.g. from Templates page)"
                onChange={(e) => { /* TODO: Bind to signatureDocumentTemplateId */ }}
             />
             <p className="text-xs text-gray-500">Required if Step Type is 'Signing'</p>
        </div>

        <Button onClick={handleSubmit} className="w-full">Save Settings</Button>
      </div>
    </Dialog>
  );
}
