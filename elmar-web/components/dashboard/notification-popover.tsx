'use client';

import { useContext, useState } from 'react';
import { Bell } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { SignalRContext } from '@/components/providers/signalr-provider';

export function NotificationPopover() {
  const { notifications } = useContext(SignalRContext);
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="relative">
      <Button className="relative p-2 bg-transparent text-gray-700 hover:bg-gray-100" onClick={() => setIsOpen(!isOpen)}>
        <Bell className="h-6 w-6" />
        {notifications.length > 0 && (
          <span className="absolute top-0 right-0 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/4 -translate-y-1/4 bg-red-600 rounded-full">
            {notifications.length}
          </span>
        )}
      </Button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 bg-white rounded-md shadow-lg overflow-hidden z-20 border">
          <div className="py-2">
            <h3 className="px-4 py-2 font-semibold border-b">Notifications</h3>
            {notifications.length === 0 ? (
              <div className="px-4 py-2 text-sm text-gray-500">No new notifications</div>
            ) : (
              notifications.map((note, idx) => (
                <div key={idx} className="px-4 py-2 text-sm text-gray-700 border-b last:border-0 hover:bg-gray-50">
                  {note}
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
