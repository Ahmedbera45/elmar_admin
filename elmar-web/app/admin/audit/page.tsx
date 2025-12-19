'use client';

import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { format } from 'date-fns';
import { Loader2, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { customInstance } from '@/lib/api/custom-instance';

interface AuditLog {
  id: string;
  tableName: string;
  action: string;
  recordId: string;
  userId?: string;
  timestamp: string;
  oldValues?: string;
  newValues?: string;
}

export default function AuditLogsPage() {
  const [userId, setUserId] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');

  const { data: logs, isLoading, refetch } = useQuery<AuditLog[]>({
    queryKey: ['audit-logs', userId, startDate, endDate],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (userId) params.append('userId', userId);
      if (startDate) params.append('startDate', startDate);
      if (endDate) params.append('endDate', endDate);

      const res = await customInstance<AuditLog[]>({
        url: `/api/audit/logs?${params.toString()}`,
        method: 'GET'
      });
      return res;
    }
  });

  return (
    <div className="container mx-auto py-10">
      <h1 className="text-3xl font-bold mb-6">Audit Logs</h1>

      <div className="flex gap-4 mb-6">
        <Input
          placeholder="User ID (optional)"
          value={userId}
          onChange={e => setUserId(e.target.value)}
          className="max-w-xs"
        />
        <Input
          type="date"
          value={startDate}
          onChange={e => setStartDate(e.target.value)}
          className="max-w-xs"
        />
        <Input
          type="date"
          value={endDate}
          onChange={e => setEndDate(e.target.value)}
          className="max-w-xs"
        />
        <Button onClick={() => refetch()}><Search className="w-4 h-4 mr-2" /> Filter</Button>
      </div>

      <div className="border rounded-lg">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Timestamp</TableHead>
              <TableHead>Action</TableHead>
              <TableHead>Table</TableHead>
              <TableHead>Record ID</TableHead>
              <TableHead>User ID</TableHead>
              <TableHead>Details</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center py-10">
                  <Loader2 className="w-8 h-8 animate-spin mx-auto" />
                </TableCell>
              </TableRow>
            ) : logs?.length === 0 ? (
                <TableRow>
                    <TableCell colSpan={6} className="text-center py-4">No logs found.</TableCell>
                </TableRow>
            ) : (
              logs?.map((log) => (
                <TableRow key={log.id}>
                  <TableCell>{format(new Date(log.timestamp), 'yyyy-MM-dd HH:mm:ss')}</TableCell>
                  <TableCell>
                    <span className={`px-2 py-1 rounded text-xs font-semibold
                      ${log.action === 'Added' || log.action === 'Create' ? 'bg-green-100 text-green-800' :
                        log.action === 'Modified' || log.action === 'Update' ? 'bg-blue-100 text-blue-800' :
                        'bg-red-100 text-red-800'}`}>
                      {log.action}
                    </span>
                  </TableCell>
                  <TableCell>{log.tableName}</TableCell>
                  <TableCell className="font-mono text-xs max-w-[150px] truncate">{log.recordId}</TableCell>
                  <TableCell className="text-xs">{log.userId || 'System'}</TableCell>
                  <TableCell>
                    <Dialog>
                      <DialogTrigger asChild>
                        <Button variant="outline" size="sm">View Changes</Button>
                      </DialogTrigger>
                      <DialogContent className="max-w-3xl max-h-[80vh] overflow-y-auto">
                        <DialogHeader>
                          <DialogTitle>Change Details</DialogTitle>
                        </DialogHeader>
                        <div className="grid grid-cols-2 gap-4">
                            <div>
                                <h4 className="font-semibold mb-2">Old Values</h4>
                                <pre className="bg-slate-100 p-2 rounded text-xs overflow-auto">
                                    {log.oldValues ? JSON.stringify(JSON.parse(log.oldValues), null, 2) : '-'}
                                </pre>
                            </div>
                            <div>
                                <h4 className="font-semibold mb-2">New Values</h4>
                                <pre className="bg-slate-100 p-2 rounded text-xs overflow-auto">
                                    {log.newValues ? JSON.stringify(JSON.parse(log.newValues), null, 2) : '-'}
                                </pre>
                            </div>
                        </div>
                      </DialogContent>
                    </Dialog>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
