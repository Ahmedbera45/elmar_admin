'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Loader2 } from 'lucide-react';
import { postUploadFile } from '@/lib/api/generated';

export interface ProcessEntry {
  key: string;
  title: string;
  entryType: 'Text' | 'Number' | 'Date' | 'Select' | 'File' | 'Checkbox';
  isRequired?: boolean;
  options?: string;
  validationRegex?: string;
}

interface DynamicFormProps {
  entries: ProcessEntry[];
  defaultValues?: any;
  onSubmit: (data: any) => void;
  submitLabel?: string;
  readOnly?: boolean;
}

export function DynamicForm({ entries, defaultValues, onSubmit, submitLabel = "Submit", readOnly = false }: DynamicFormProps) {
  const { register, handleSubmit, setValue, formState: { errors, isSubmitting } } = useForm({
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

        return (
          <div key={entry.key} className="space-y-2">
            <Label htmlFor={entry.key}>{entry.title} {!readOnly && entry.isRequired && <span className="text-red-500">*</span>}</Label>

            {entry.entryType === 'Text' && (
              <Input
                id={entry.key}
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired })}
              />
            )}

            {entry.entryType === 'Number' && (
              <Input
                id={entry.key}
                type="number"
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired, valueAsNumber: true })}
              />
            )}

            {entry.entryType === 'Date' && (
              <Input
                id={entry.key}
                type="date"
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired })}
              />
            )}

            {entry.entryType === 'Select' && (
              <Select
                id={entry.key}
                options={options}
                disabled={readOnly}
                {...register(entry.key, { required: !readOnly && entry.isRequired })}
              />
            )}

            {entry.entryType === 'File' && (
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
