import { useQuery } from '@tanstack/react-query';
import { AXIOS_INSTANCE } from '@/lib/axios-instance';

export const useGetProcessViewDefinition = (code: string) => {
  return useQuery({
    queryKey: ['process-view', code],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get(`/api/workflow/process/${code}/view-definition`);
      return res.data;
    }
  });
};

export const useGetProcessRequests = (code: string) => {
  return useQuery({
    queryKey: ['process-requests', code],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get(`/api/workflow/process/${code}/requests`);
      return res.data;
    }
  });
};

export const useGetRequest = (id: string) => {
  return useQuery({
    queryKey: ['request', id],
    queryFn: async () => {
        const res = await AXIOS_INSTANCE.get(`/api/workflow/request/${id}`);
        return res.data;
    }
  });
}

export const useGetRequestForm = (id: string) => {
    return useQuery({
        queryKey: ['request-form', id],
        queryFn: async () => {
            const res = await AXIOS_INSTANCE.get(`/api/workflow/request/${id}/form`);
            return res.data;
        }
    });
}

export const postStartProcess = async (processCode: string) => {
    const res = await AXIOS_INSTANCE.post(`/api/workflow/start?processCode=${processCode}`);
    return res.data;
}

export const postExecuteAction = async (data: any) => {
    const res = await AXIOS_INSTANCE.post('/api/workflow/execute', data);
    return res.data;
}
