'use client';

import { useGetTemplates } from '@/lib/api/generated';
import { Button } from '@/components/ui/button';
import { Card, CardHeader, CardTitle } from '@/components/ui/card';
import Link from 'next/link';

export default function TemplatesPage() {
  const { data: templates, isLoading } = useGetTemplates();

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">Document Templates</h1>
        <Link href="/dashboard/admin/templates/new/design">
           <Button>New Template</Button>
        </Link>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        {templates?.map((t: any) => (
          <Card key={t.id}>
            <CardHeader className="flex flex-row justify-between items-center">
              <CardTitle>{t.name}</CardTitle>
              <Link href={`/dashboard/admin/templates/${t.id}/design`}>
                 <Button variant="outline" size="sm">Design</Button>
              </Link>
            </CardHeader>
          </Card>
        ))}
      </div>
    </div>
  );
}
