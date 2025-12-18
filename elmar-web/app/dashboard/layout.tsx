import { LayoutDashboard, FileText, Settings, LogOut } from 'lucide-react';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen bg-gray-100">
      <aside className="w-64 bg-white shadow-md">
        <div className="flex h-16 items-center justify-center border-b px-6">
          <span className="text-xl font-bold text-gray-800">Elmar</span>
        </div>
        <nav className="mt-6 px-4 space-y-2">
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
          <div className="pt-4 mt-4 border-t">
             <a href="/login" className="flex items-center rounded-md px-4 py-2 text-red-600 hover:bg-red-50">
                <LogOut className="mr-3 h-5 w-5" />
                Logout
             </a>
          </div>
        </nav>
      </aside>
      <main className="flex-1 p-8">
        {children}
      </main>
    </div>
  );
}
