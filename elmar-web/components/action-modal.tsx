'use client';

import { Dialog } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { useState } from 'react';

interface ActionModalProps {
  isOpen: boolean;
  onClose: () => void;
  action: { id: string; name: string; actionType: any; isCommentRequired: boolean } | null;
  onSubmit: (actionId: string, comments: string) => void;
}

export function ActionModal({ isOpen, onClose, action, onSubmit }: ActionModalProps) {
  const [comments, setComments] = useState('');

  const handleSubmit = () => {
    if (action?.isCommentRequired && !comments.trim()) {
      alert('Comment is required');
      return;
    }
    if (action) {
      onSubmit(action.id, comments);
      setComments('');
      onClose();
    }
  };

  if (!action) return null;

  return (
    <Dialog isOpen={isOpen} onClose={onClose} title={action.name}>
      <div className="space-y-4">
        <p>Are you sure you want to perform this action?</p>
        <div>
          <label className="block text-sm font-medium mb-1">Comments {action.isCommentRequired && '*'}</label>
          <Textarea
            value={comments}
            onChange={(e) => setComments(e.target.value)}
            placeholder="Enter comments here..."
          />
        </div>
        <div className="flex justify-end space-x-2">
          <Button onClick={onClose} className="bg-gray-500 hover:bg-gray-600">Cancel</Button>
          <Button
            onClick={handleSubmit}
            className={action.name.toLowerCase().includes('reject') ? 'bg-red-600 hover:bg-red-700' : 'bg-green-600 hover:bg-green-700'}
          >
            Confirm
          </Button>
        </div>
      </div>
    </Dialog>
  );
}
