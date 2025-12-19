'use client';

import React, { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Loader2 } from 'lucide-react';
import { postUploadFile, useGetUsers } from '@/lib/api/generated';

export interface ProcessEntry {
  key: string;
  title: string;
  entryType: 'Text' | 'Number' | 'Date' | 'Select' | 'File' | 'Checkbox' | 'UserSelect';
  isRequired?: boolean;
  options?: string;
  validationRegex?: string;
  lookupSource?: string;
}

interface DynamicFormProps {
  entries: ProcessEntry[];
  defaultValues?: any;
  onSubmit: (data: any) => void;
  submitLabel?: string;
  readOnly?: boolean;
}

function UserSelectField({ entry, control, readOnly }: { entry: ProcessEntry, control: any, readOnly: boolean }) {
    const { data: users, isLoading } = useGetUsers(entry.lookupSource || undefined);

    if (readOnly) {
         return <Input disabled value={control._formValues[entry.key] || ''} />;
    }

    return (
        <Controller
            name={entry.key}
            control={control}
            rules={{ required: entry.isRequired }}
            render={({ field }) => (
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                    <SelectTrigger>
                        <SelectValue placeholder="Select User" />
                    </SelectTrigger>
                    <SelectContent>
                        {isLoading ? <SelectItem value="loading" disabled>Loading users...</SelectItem> :
                         users?.map((u: any) => (
                             <SelectItem key={u.id} value={u.id}>{u.username} ({u.role})</SelectItem>
                         ))}
                    </SelectContent>
                </Select>
            )}
        />
    );
}

export function DynamicForm({ entries, defaultValues, onSubmit, submitLabel = "Submit", readOnly = false }: DynamicFormProps) {
  const { register, control, handleSubmit, setValue, formState: { errors, isSubmitting } } = useForm({
    defaultValues: defaultValues || {},
  });
  const [uploading, setUploading] = useState<Record<string, boolean>>({});

  const handleFileChange = async (key: string, e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(prev => ({ ...prev, [key]: true }));
    try {
      const res = await postUploadFile(file);
      setValue(key, res.fileId);
    } catch (err) {
      console.error(err);
      alert("Upload failed");
    } finally {
      setUploading(prev => ({ ...prev, [key]: false }));
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {entries.map((entry) => {
        let options = [];
        if (entry.entryType === 'Select' && entry.options) {
          try {
            options = JSON.parse(entry.options);
          } catch (e) {
            console.error('Failed to parse options for', entry.key);
          }
        }

        // Map integer types to string if necessary, assuming entryType comes as string from API DTO if I mapped it,
        // OR handle 1/2/3/7 integers if DTO is raw.
        // Based on ProcessFieldModal, we treat them as numbers mostly but Admin Service DTO returns Enum (int).
        // Frontend TS interface says string literal union.
        // Let's assume the API conversion happens or we handle the int value.
        // Actually, generated.ts usually types them as any or number if not specified.
        // I'll assume we need to handle the numeric values or match the string if mapped.
        // Previous code used strings. I'll stick to strings for the check if mapped, or use the number.
        // The interface defines strings. I should map 7 to 'UserSelect'.
        // However, I can't easily change the incoming data type without a mapper.
        // I'll check `entryType` loosely.
        const type: any = entry.entryType;

        return (
          <div key={entry.key} className="space-y-2">
            <Label htmlFor={entry.key}>{entry.title} {!readOnly && entry.isRequired && <span className="text-red-500">*</span>}</Label>

            {(type === 'Text' || type === 1) && (
              <Input
                id={entry.key}
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired })}
              />
            )}

            {(type === 'Number' || type === 2) && (
              <Input
                id={entry.key}
                type="number"
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired, valueAsNumber: true })}
              />
            )}

            {(type === 'Date' || type === 3) && (
              <Input
                id={entry.key}
                type="date"
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired })}
              />
            )}

            {(type === 'Select' || type === 4) && (
                readOnly ? <Input disabled value={defaultValues?.[entry.key]} /> :
                <Controller
                    name={entry.key}
                    control={control}
                    rules={{ required: entry.isRequired }}
                    render={({ field }) => (
                        <Select onValueChange={field.onChange} defaultValue={field.value}>
                            <SelectTrigger>
                                <SelectValue placeholder="Select..." />
                            </SelectTrigger>
                            <SelectContent>
                                {options.map((opt: any) => (
                                    <SelectItem key={opt.value} value={opt.value}>{opt.label}</SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    )}
                />
            )}

            {(type === 'File' || type === 5) && (
              <div className="space-y-1">
                <Input
                  type="file"
                  disabled={readOnly || uploading[entry.key]}
                  onChange={(e) => handleFileChange(entry.key, e)}
                />
                <input type="hidden" {...register(entry.key, { required: !readOnly && entry.isRequired })} />
                {uploading[entry.key] && <span className="text-sm text-muted-foreground animate-pulse">Uploading...</span>}
              </div>
            )}

            {(type === 'Checkbox' || type === 6) && (
                 <div className="flex items-center space-x-2">
                    <input
                        type="checkbox"
                        disabled={readOnly}
                        {...register(entry.key)}
                        className="h-4 w-4"
                    />
                    <span className="text-sm text-gray-700">{entry.title}</span>
                 </div>
            )}

            {(type === 'UserSelect' || type === 7) && (
                <UserSelectField entry={entry} control={control} readOnly={readOnly} />
            )}

            {!readOnly && errors[entry.key] && (
              <span className="text-sm text-red-500">This field is required</span>
            )}
          </div>
        );
      })}

      {!readOnly && (
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {submitLabel}
        </Button>
      )}
    </form>
  );
}
