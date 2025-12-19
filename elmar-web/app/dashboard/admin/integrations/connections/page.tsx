'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { customInstance } from '@/lib/api/custom-instance';
import { Loader2 } from 'lucide-react';

export default function ConnectionsPage() {
    const { data: connections, refetch } = useQuery<any[]>({
        queryKey: ['connections'],
        queryFn: () => customInstance({ url: '/api/integration/connections', method: 'GET' })
    });

    const [form, setForm] = useState({ name: '', provider: 'MSSQL', connectionString: '' });
    const [loading, setLoading] = useState(false);

    const handleCreate = async () => {
        setLoading(true);
        try {
            await customInstance({ url: '/api/integration/connections', method: 'POST', data: form });
            setForm({ name: '', provider: 'MSSQL', connectionString: '' });
            refetch();
        } finally { setLoading(false); }
    };

    const handleTest = async (id: string) => {
        const res = await customInstance<{success: boolean}>({ url: `/api/integration/connections/${id}/test`, method: 'POST' });
        alert(res.success ? "Connection OK" : "Connection Failed");
    };

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-6">External Connections</h1>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white p-4 rounded shadow">
                    <h3 className="text-lg font-bold mb-4">Add Connection</h3>
                    <div className="space-y-4">
                        <div>
                            <Label>Name</Label>
                            <Input value={form.name} onChange={e => setForm({...form, name: e.target.value})} />
                        </div>
                        <div>
                            <Label>Provider</Label>
                            <Select value={form.provider} onValueChange={v => setForm({...form, provider: v})}>
                                <SelectTrigger><SelectValue /></SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="MSSQL">MSSQL</SelectItem>
                                    <SelectItem value="PostgreSQL">PostgreSQL</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div>
                            <Label>Connection String</Label>
                            <Input value={form.connectionString} onChange={e => setForm({...form, connectionString: e.target.value})} />
                        </div>
                        <Button onClick={handleCreate} disabled={loading}>{loading ? <Loader2 className="animate-spin" /> : 'Save'}</Button>
                    </div>
                </div>

                <div className="bg-white p-4 rounded shadow">
                    <h3 className="text-lg font-bold mb-4">Existing Connections</h3>
                    <div className="space-y-2">
                        {connections?.map((c: any) => (
                            <div key={c.id} className="flex justify-between items-center p-2 border rounded">
                                <div>
                                    <div className="font-semibold">{c.name}</div>
                                    <div className="text-xs text-gray-500">{c.provider}</div>
                                </div>
                                <Button variant="outline" size="sm" onClick={() => handleTest(c.id)}>Test</Button>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}
