'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { DynamicForm, ProcessEntry } from '@/components/dynamic-form';
import { AXIOS_INSTANCE } from '@/lib/axios-instance';
import { postStartProcess, postExecuteAction } from '@/lib/api/generated';

export default function StartProcessPage({ params }: { params: { code: string } }) {
  const router = useRouter();
  const [requestId, setRequestId] = useState<string | null>(null);
  const [entries, setEntries] = useState<ProcessEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [requestDetails, setRequestDetails] = useState<any>(null);

  useEffect(() => {
    const init = async () => {
      try {
        const startRes = await postStartProcess(params.code);
        const id = startRes.requestId;
        setRequestId(id);

        const formRes = await AXIOS_INSTANCE.get(`/api/workflow/request/${id}/form`);
        setEntries(formRes.data);

        const reqRes = await AXIOS_INSTANCE.get(`/api/workflow/request/${id}`);
        setRequestDetails(reqRes.data);

        setLoading(false);
      } catch (err) {
        console.error("Failed to init process", err);
        alert("Failed to start process");
      }
    };

    init();
  }, [params.code]);

  const handleSubmit = async (data: any) => {
    if (!requestId || !requestDetails) return;

    const action = requestDetails.currentStep.actions[0];
    if (!action) {
      alert("No action found for this step.");
      return;
    }

    try {
      await postExecuteAction({
        requestId: requestId,
        actionId: action.id,
        formValues: data,
        comments: "Started via Web"
      });
      router.push(`/dashboard/processes/${params.code}`);
    } catch (err) {
      console.error(err);
      alert("Failed to submit form");
    }
  };

  if (loading) return <div>Initializing Process...</div>;

  return (
    <div className="max-w-2xl mx-auto py-10">
      <h1 className="text-2xl font-bold mb-6">New Request: {params.code}</h1>
      <div className="bg-white p-6 rounded-lg shadow">
        <DynamicForm
          entries={entries}
          onSubmit={handleSubmit}
          submitLabel={requestDetails?.currentStep?.actions?.[0]?.name || "Submit"}
        />
      </div>
    </div>
  );
}
