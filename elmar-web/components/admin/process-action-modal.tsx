'use client';

import { useState } from 'react';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ConditionBuilder } from '@/components/admin/condition-builder';
import { postAddAction } from '@/lib/api/generated';
import { useToast } from '@/components/ui/toast-context';

interface ProcessActionModalProps {
  isOpen: boolean;
  onClose: () => void;
  stepId: string;
  steps: { id: string; name: string }[];
  allFields: any[];
  onSuccess: () => void;
}

export function ProcessActionModal({ isOpen, onClose, stepId, steps, allFields, onSuccess }: ProcessActionModalProps) {
  const [activeTab, setActiveTab] = useState('general');
  const [formData, setFormData] = useState({
    name: '',
    actionType: 1,
    targetStepId: '',
    webhookUrl: '',
    webhookMethod: 'POST',
    ruleExpression: ''
  });
  const { toast } = useToast();

  const handleSubmit = async () => {
    try {
      await postAddAction({
        stepId: stepId,
        name: formData.name,
        actionType: Number(formData.actionType),
        targetStepId: formData.targetStepId || null,
        webhookUrl: formData.webhookUrl,
        webhookMethod: formData.webhookMethod,
        ruleExpression: formData.ruleExpression
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
      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="general">General</TabsTrigger>
          <TabsTrigger value="fintech">Fintech & Legal</TabsTrigger>
          <TabsTrigger value="advanced">Advanced</TabsTrigger>
        </TabsList>
        <div className="mt-4">
            <TabsContent value="general" className="space-y-4">
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
            </TabsContent>

            <TabsContent value="advanced" className="space-y-4">
                <div className="space-y-4 border rounded p-3">
                    <h4 className="font-medium text-sm">Visual Condition Builder</h4>
                    <p className="text-xs text-gray-500">Only execute this transition if:</p>
                    <ConditionBuilder
                        fields={allFields}
                        value={formData.ruleExpression}
                        onChange={val => setFormData({...formData, ruleExpression: val})}
                    />
                </div>

                <div className="space-y-4 border rounded p-3">
                    <h4 className="font-medium text-sm">Webhook Integration</h4>
                    <div>
                        <Label>Webhook URL</Label>
                        <Input
                            value={formData.webhookUrl}
                            onChange={e => setFormData({...formData, webhookUrl: e.target.value})}
                            placeholder="https://api.external.com/notify"
                        />
                    </div>
                    <div>
                        <Label>Method</Label>
                        <Select
                            value={formData.webhookMethod}
                            onChange={e => setFormData({...formData, webhookMethod: e.target.value})}
                            options={[{ label: 'POST', value: 'POST' }, { label: 'GET', value: 'GET' }]}
                        />
                    </div>
                </div>
            </TabsContent>
        </div>
        <div className="mt-4">
             <Button onClick={handleSubmit} className="w-full">Create Action</Button>
        </div>
      </Tabs>
    </Dialog>
  );
}
