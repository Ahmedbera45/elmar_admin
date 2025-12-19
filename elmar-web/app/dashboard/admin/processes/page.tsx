'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useGetProcesses, postCreateProcess } from '@/lib/api/generated';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/toast-context';

export default function AdminProcessListPage() {
  const { data: processes, isLoading, refetch } = useGetProcesses();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newProcess, setNewProcess] = useState({ name: '', code: '' });
  const { toast } = useToast();

  const handleCreate = async () => {
    try {
      await postCreateProcess(newProcess);
      toast("Process created successfully");
      setIsModalOpen(false);
      refetch();
    } catch (e) {
      toast("Failed to create process", "error");
    }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Process Management</h1>
        <Button onClick={() => setIsModalOpen(true)}>New Process</Button>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {processes?.map((proc: any) => (
          <Link key={proc.id} href={`/dashboard/admin/processes/${proc.id}/design`}>
            <Card className="hover:shadow-lg transition-shadow cursor-pointer">
              <CardHeader>
                <CardTitle>{proc.name}</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-gray-500">Code: {proc.code}</p>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      <Dialog isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="New Process">
        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium">Name</label>
            <Input value={newProcess.name} onChange={e => setNewProcess({...newProcess, name: e.target.value})} />
          </div>
          <div>
            <label className="text-sm font-medium">Code</label>
            <Input value={newProcess.code} onChange={e => setNewProcess({...newProcess, code: e.target.value})} />
          </div>
          <Button onClick={handleCreate} className="w-full">Create</Button>
        </div>
      </Dialog>
    </div>
  );
}
