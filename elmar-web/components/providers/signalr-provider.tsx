'use client';

import { useEffect, useState, createContext } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useToast } from '@/components/ui/toast-context';
import { useQueryClient } from '@tanstack/react-query';

export const SignalRContext = createContext<{ notifications: string[] }>({ notifications: [] });

export function SignalRProvider({ children }: { children: React.ReactNode }) {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [notifications, setNotifications] = useState<string[]>([]);
  const { toast } = useToast();
  const queryClient = useQueryClient();

  useEffect(() => {
    const token = localStorage.getItem('token');

    const newConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/hub/notifications', {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    setConnection(newConnection);
  }, []);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => console.log('SignalR Connected'))
        .catch(err => console.error('SignalR Connection Error: ', err));

      connection.on('ReceiveNotification', (message: string) => {
        toast(message, 'success');
        setNotifications(prev => [message, ...prev].slice(0, 10));
      });

      connection.on('ReceiveUpdate', () => {
        console.log("Received update signal");
        queryClient.invalidateQueries();
      });

      return () => {
        connection.off('ReceiveNotification');
        connection.off('ReceiveUpdate');
        connection.stop();
      };
    }
  }, [connection, toast, queryClient]);

  return (
    <SignalRContext.Provider value={{ notifications }}>
      {children}
    </SignalRContext.Provider>
  );
}
