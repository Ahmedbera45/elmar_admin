'use client';

import { LayoutDashboard, FileText, Settings, LogOut, Shield } from 'lucide-react';
import { NotificationPopover } from '@/components/dashboard/notification-popover';
import { useEffect, useState } from 'react';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const [isAdmin, setIsAdmin] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            if (payload.role === 'Admin') setIsAdmin(true);
        } catch (e) {}
    }
  }, []);

  return (
    <div className="flex min-h-screen bg-gray-100">
      <aside className="w-64 bg-white shadow-md flex flex-col">
        <div className="flex h-16 items-center justify-center border-b px-6">
          <span className="text-xl font-bold text-gray-800">Elmar</span>
        </div>
        <nav className="mt-6 px-4 space-y-2 flex-1">
          <a href="/dashboard" className="flex items-center rounded-md px-4 py-2 text-gray-700 hover:bg-gray-100">
            <LayoutDashboard className="mr-3 h-5 w-5" />
            Dashboard
          </a>
          <a href="#" className="flex items-center rounded-md px-4 py-2 text-gray-700 hover:bg-gray-100">
            <FileText className="mr-3 h-5 w-5" />
            Processes
          </a>
          <a href="#" className="flex items-center rounded-md px-4 py-2 text-gray-700 hover:bg-gray-100">
            <Settings className="mr-3 h-5 w-5" />
            Settings
          </a>

          {isAdmin && (
            <a href="/dashboard/admin/processes" className="flex items-center rounded-md px-4 py-2 text-gray-700 hover:bg-gray-100 mt-4 border-t pt-4">
                <Shield className="mr-3 h-5 w-5" />
                Process Management
            </a>
          )}

          <div className="pt-4 mt-4 border-t">
             <a href="/login" className="flex items-center rounded-md px-4 py-2 text-red-600 hover:bg-red-50">
                <LogOut className="mr-3 h-5 w-5" />
                Logout
             </a>
          </div>
        </nav>
      </aside>
      <div className="flex-1 flex flex-col">
        <header className="h-16 bg-white shadow-sm flex items-center justify-end px-8">
            <NotificationPopover />
        </header>
        <main className="flex-1 p-8 overflow-y-auto">
            {children}
        </main>
      </div>
    </div>
  );
}
