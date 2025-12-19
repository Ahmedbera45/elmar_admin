'use client';

import { StatsCards } from '@/components/dashboard/stats-cards';
import { ApplicationChart } from '@/components/dashboard/application-chart';
import { useGetDashboardStats, useGetChartData } from '@/lib/api/generated';

export default function DashboardPage() {
  const { data: stats, isLoading: loadingStats } = useGetDashboardStats();
  const { data: chartData, isLoading: loadingChart } = useGetChartData();

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Dashboard</h1>

      <StatsCards stats={stats} isLoading={loadingStats} />

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
        <div className="col-span-4">
          <ApplicationChart data={chartData} isLoading={loadingChart} />
        </div>
      </div>
    </div>
  );
}
