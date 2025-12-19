'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { useGetProcessViewDefinition, useGetProcessRequests, ProcessRequestFilter } from '@/lib/api/generated';
import { RequestFilters } from '@/components/process/request-filters';
import { StatusBadge } from '@/components/ui/status-badge';
import { useRouter } from 'next/navigation';

export default function ProcessListPage({ params }: { params: { code: string } }) {
  const router = useRouter();
  const [filter, setFilter] = useState<ProcessRequestFilter>({});

  const { data: viewDef, isLoading: loadingView } = useGetProcessViewDefinition(params.code);
  const { data: requests, isLoading: loadingReqs } = useGetProcessRequests(params.code, filter);

  if (loadingView) return <div className="p-8 text-center text-gray-500">Loading definition...</div>;

  return (
    <div className="space-y-6 fade-in p-4">
      <div className="flex justify-between items-center">
        <div>
           <h1 className="text-2xl font-bold tracking-tight">{viewDef?.processTitle || params.code}</h1>
           <p className="text-gray-500 text-sm">Manage and track your requests</p>
        </div>
        <Link href={`/dashboard/processes/${params.code}/start`}>
          <Button>New Request</Button>
        </Link>
      </div>

      <RequestFilters onFilterChange={setFilter} />

      {loadingReqs ? (
          <div className="p-12 text-center text-gray-400 animate-pulse">Loading requests...</div>
      ) : requests && requests.length > 0 ? (
        <div className="rounded-lg border bg-white shadow-sm overflow-hidden">
            <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
            <thead className="bg-gray-50 border-b">
                <tr>
                <th className="px-4 py-3 font-medium text-gray-500">Request ID</th>
                <th className="px-4 py-3 font-medium text-gray-500">Status</th>
                <th className="px-4 py-3 font-medium text-gray-500">Created At</th>
                {viewDef?.columns?.map((col: any) => (
                    <th key={col.key} className="px-4 py-3 font-medium text-gray-500">{col.title}</th>
                ))}
                <th className="px-4 py-3 font-medium text-gray-500 text-right">Action</th>
                </tr>
            </thead>
            <tbody>
                {requests.map((req: any) => (
                <tr
                    key={req.id}
                    className="border-b last:border-0 hover:bg-gray-50 transition-colors cursor-pointer"
                    onClick={() => router.push(`/dashboard/requests/${req.id}`)}
                >
                    <td className="px-4 py-3 font-mono text-xs text-gray-600">{req.id.substring(0,8)}...</td>
                    <td className="px-4 py-3">
                        <StatusBadge status={req.status} />
                    </td>
                    <td className="px-4 py-3 text-gray-600">{new Date(req.createdAt).toLocaleDateString()}</td>
                    {viewDef?.columns?.map((col: any) => (
                    <td key={col.key} className="px-4 py-3 max-w-[200px] truncate" title={req.dynamicValues?.[col.key]?.toString()}>
                        {req.dynamicValues?.[col.key]?.toString() || '-'}
                    </td>
                    ))}
                    <td className="px-4 py-3 text-right">
                        <Button variant="ghost" size="sm">View</Button>
                    </td>
                </tr>
                ))}
            </tbody>
            </table>
            </div>
        </div>
      ) : (
          <div className="text-center py-16 border rounded-lg border-dashed bg-gray-50">
              <h3 className="text-lg font-medium text-gray-900">No requests found</h3>
              <p className="text-gray-500 mt-1">Try adjusting your filters or create a new request.</p>
          </div>
      )}
    </div>
  );
}
