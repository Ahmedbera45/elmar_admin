'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { ToastProvider } from '@/components/ui/toast-context';
import { SignalRProvider } from '@/components/providers/signalr-provider';

export default function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(() => new QueryClient());

  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <SignalRProvider>
          {children}
        </SignalRProvider>
      </ToastProvider>
    </QueryClientProvider>
  );
}
