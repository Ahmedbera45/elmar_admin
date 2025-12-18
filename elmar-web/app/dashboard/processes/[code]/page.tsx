'use client';

import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { useGetProcessViewDefinition, useGetProcessRequests } from '@/lib/api/generated';

export default function ProcessListPage({ params }: { params: { code: string } }) {
  const { data: viewDef, isLoading: loadingView } = useGetProcessViewDefinition(params.code);
  const { data: requests, isLoading: loadingReqs } = useGetProcessRequests(params.code);

  if (loadingView || loadingReqs) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">{viewDef?.processTitle || params.code}</h1>
        <Link href={`/dashboard/processes/${params.code}/start`}>
          <Button>New Request</Button>
        </Link>
      </div>

      <div className="rounded-md border bg-white">
        <table className="w-full text-sm text-left">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="px-4 py-3 font-medium">ID</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 font-medium">Date</th>
              {viewDef?.columns?.map((col: any) => (
                <th key={col.key} className="px-4 py-3 font-medium">{col.title}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {requests?.map((req: any) => (
              <tr key={req.id} className="border-b last:border-0 hover:bg-gray-50">
                <td className="px-4 py-3">{req.id.substring(0,8)}...</td>
                <td className="px-4 py-3">{req.status}</td>
                <td className="px-4 py-3">{new Date(req.createdAt).toLocaleDateString()}</td>
                {viewDef?.columns?.map((col: any) => (
                  <td key={col.key} className="px-4 py-3">
                    {req.dynamicValues?.[col.key]?.toString() || '-'}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
