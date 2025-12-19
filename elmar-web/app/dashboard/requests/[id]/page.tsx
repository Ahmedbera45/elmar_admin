'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { AXIOS_INSTANCE } from '@/lib/axios-instance';
import { useQuery } from '@tanstack/react-query';
import { DynamicForm } from '@/components/dynamic-form';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ActionModal } from '@/components/action-modal';
import { postExecuteAction } from '@/lib/api/generated';
import { useToast } from '@/components/ui/toast-context';
import { CheckCircle, XCircle, Clock } from 'lucide-react';

const useGetRequestDetail = (id: string) => {
  return useQuery({
    queryKey: ['request-detail', id],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get(`/api/workflow/request/${id}/detail`);
      return res.data;
    }
  });
};

const useGetRequestFormDef = (id: string) => {
  return useQuery({
    queryKey: ['request-form-def', id],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get(`/api/workflow/request/${id}/form`);
      return res.data;
    }
  });
}

export default function RequestDetailPage({ params }: { params: { id: string } }) {
  const router = useRouter();
  const { toast } = useToast();
  const { data: detail, isLoading: loadingDetail, refetch } = useGetRequestDetail(params.id);
  const { data: formEntries, isLoading: loadingForm } = useGetRequestFormDef(params.id);

  const [selectedAction, setSelectedAction] = useState<any>(null);

  if (loadingDetail || loadingForm) return <div>Loading...</div>;
  if (!detail) return <div>Request not found</div>;

  const handleActionClick = (action: any) => {
    setSelectedAction(action);
  };

  const handleActionSubmit = async (actionId: string, comments: string) => {
    try {
      await postExecuteAction({
        requestId: params.id,
        actionId: actionId,
        formValues: detail.formValues,
        comments: comments
      });
      toast("Action executed successfully");
      refetch();
    } catch (err) {
      console.error(err);
      toast("Failed to execute action", "error");
    }
  };

  return (
    <div className="space-y-6 max-w-5xl mx-auto py-6">
      <Card>
        <CardHeader className="flex flex-row justify-between items-center">
          <div className="space-y-1">
            <CardTitle>{detail.requestNumber}</CardTitle>
            <p className="text-sm text-muted-foreground">{detail.processName} - {detail.currentStepName}</p>
          </div>
          <div className="text-right">
            <div className="text-sm font-medium">{detail.status === 1 ? 'Active' : 'Completed'}</div>
            <div className="text-xs text-muted-foreground">{new Date(detail.createdAt).toLocaleDateString()}</div>
            <div className="text-xs text-muted-foreground">by {detail.initiatorName}</div>
          </div>
        </CardHeader>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle>Application Data</CardTitle>
            </CardHeader>
            <CardContent>
              {formEntries && (
                <DynamicForm
                  entries={formEntries}
                  defaultValues={detail.formValues}
                  onSubmit={() => {}}
                  readOnly={true}
                />
              )}
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          {detail.nextActions?.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Actions</CardTitle>
              </CardHeader>
              <CardContent className="flex flex-col space-y-2">
                {detail.nextActions.map((action: any) => (
                  <Button
                    key={action.id}
                    onClick={() => handleActionClick(action)}
                    className={action.name.toLowerCase().includes('reject') ? 'bg-red-600 hover:bg-red-700' : 'bg-green-600 hover:bg-green-700'}
                  >
                    {action.name}
                  </Button>
                ))}
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader>
              <CardTitle>History</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="relative border-l border-gray-200 dark:border-gray-700 ml-2">
                {detail.history?.map((hist: any, index: number) => (
                  <div key={index} className="mb-8 ml-6">
                    <div className="absolute w-6 h-6 bg-white rounded-full -left-3 border border-gray-200 flex items-center justify-center">
                        {hist.actionName.includes('Approve') ? <CheckCircle className="w-4 h-4 text-green-500" /> :
                         hist.actionName.includes('Reject') ? <XCircle className="w-4 h-4 text-red-500" /> :
                         <Clock className="w-4 h-4 text-gray-500" />}
                    </div>
                    <time className="mb-1 text-sm font-normal leading-none text-gray-400 dark:text-gray-500">{new Date(hist.createdAt).toLocaleString()}</time>
                    <h3 className="text-sm font-semibold text-gray-900 dark:text-white">{hist.actionName}</h3>
                    <p className="mb-1 text-sm font-normal text-gray-500 dark:text-gray-400">{hist.actorName}</p>
                    <p className="text-sm italic text-gray-500">{hist.description}</p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      <ActionModal
        isOpen={!!selectedAction}
        onClose={() => setSelectedAction(null)}
        action={selectedAction}
        onSubmit={handleActionSubmit}
      />
    </div>
  );
}
