'use client';

import { useState, useEffect } from 'react';
import { useGetSettings, postUpdateSetting } from '@/lib/api/generated';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { useToast } from '@/components/ui/toast-context';

export default function SettingsPage() {
  const { data: settings, refetch, isLoading } = useGetSettings();
  const [localSettings, setLocalSettings] = useState<any[]>([]);
  const { toast } = useToast();

  useEffect(() => {
    if (settings) {
      setLocalSettings(settings);
    }
  }, [settings]);

  const handleUpdate = async (setting: any) => {
    try {
      await postUpdateSetting(setting);
      toast("Setting updated");
      refetch();
    } catch (e) {
      toast("Error updating setting", "error");
    }
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">System Settings</h1>

      <Card>
        <CardContent className="space-y-4 pt-6">
            {localSettings.map((setting, index) => (
                <div key={setting.key} className="grid grid-cols-1 md:grid-cols-3 gap-4 items-end border-b pb-4 last:border-0">
                    <div>
                        <Label>{setting.key}</Label>
                        <p className="text-xs text-gray-500">{setting.description}</p>
                    </div>
                    <div>
                        <Input
                            value={setting.value || ''}
                            onChange={(e) => {
                                const newSettings = [...localSettings];
                                newSettings[index].value = e.target.value;
                                setLocalSettings(newSettings);
                            }}
                        />
                    </div>
                    <div>
                        <Button size="sm" onClick={() => handleUpdate(setting)}>Save</Button>
                    </div>
                </div>
            ))}
            {localSettings.length === 0 && <p>No settings found.</p>}
        </CardContent>
      </Card>

      <div className="p-4 bg-gray-50 rounded text-sm text-gray-600">
          <p>Note: Use the API to add new keys initially if not present.</p>
      </div>
    </div>
  );
}
