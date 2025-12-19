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

export const postUploadFile = async (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await AXIOS_INSTANCE.post('/api/files/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
}

export type ProcessRequestFilter = {
  status?: number;
  startDate?: string;
  endDate?: string;
};

export const useGetProcessRequests = (code: string, filter?: ProcessRequestFilter) => {
  return useQuery({
    queryKey: ['process-requests', code, filter],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (filter?.status !== undefined) params.append('status', filter.status.toString());
      if (filter?.startDate) params.append('startDate', filter.startDate);
      if (filter?.endDate) params.append('endDate', filter.endDate);

      const res = await AXIOS_INSTANCE.get(`/api/workflow/process/${code}/requests`, { params });
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

// Admin Mock Hooks
export const useGetProcesses = () => {
  return useQuery({
    queryKey: ['admin-processes'],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get('/api/admin/processes');
      return res.data;
    }
  });
};

export const useGetProcessDefinition = (id: string) => {
  return useQuery({
    queryKey: ['admin-process-def', id],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get(`/api/admin/process/${id}`);
      return res.data;
    }
  });
};

export const postCreateProcess = async (data: any) => {
    const res = await AXIOS_INSTANCE.post('/api/admin/process', data);
    return res.data;
}

export const postAddStep = async (data: any) => {
    const res = await AXIOS_INSTANCE.post('/api/admin/step', data);
    return res.data;
}

export const postAddAction = async (data: any) => {
    const res = await AXIOS_INSTANCE.post('/api/admin/action', data);
    return res.data;
}

export const postAddField = async (data: any) => {
    const res = await AXIOS_INSTANCE.post('/api/admin/field', data);
    return res.data;
}

export const useGetDashboardStats = () => {
  return useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get('/api/dashboard/stats');
      return res.data;
    }
  });
};

export const useGetChartData = () => {
  return useQuery({
    queryKey: ['dashboard-chart'],
    queryFn: async () => {
      const res = await AXIOS_INSTANCE.get('/api/dashboard/chart-data');
      return res.data;
    }
  });
};
