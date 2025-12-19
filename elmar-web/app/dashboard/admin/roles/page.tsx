'use client';

import { useState } from 'react';
import { useGetRoles, postCreateRole, deleteRole } from '@/lib/api/generated';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Trash } from 'lucide-react';
import { useToast } from '@/components/ui/toast-context';

export default function RolesPage() {
  const { data: roles, refetch, isLoading } = useGetRoles();
  const [newRole, setNewRole] = useState('');
  const { toast } = useToast();

  const handleAdd = async () => {
    if (!newRole) return;
    try {
      await postCreateRole({ name: newRole });
      setNewRole('');
      toast("Role created");
      refetch();
    } catch (e) {
      toast("Error creating role", "error");
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure?')) return;
    try {
      await deleteRole(id);
      toast("Role deleted");
      refetch();
    } catch (e) {
      toast("Error deleting role", "error");
    }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Role Management</h1>

      <div className="flex gap-4">
        <Input
          value={newRole}
          onChange={(e) => setNewRole(e.target.value)}
          placeholder="New Role Name"
          className="max-w-xs"
        />
        <Button onClick={handleAdd}>Add Role</Button>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {roles?.map((role: any) => (
          <Card key={role.id}>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{role.name}</CardTitle>
              <Button variant="ghost" size="icon" onClick={() => handleDelete(role.id)}>
                <Trash className="h-4 w-4 text-red-500" />
              </Button>
            </CardHeader>
          </Card>
        ))}
      </div>
    </div>
  );
}
