'use client';

import React from 'react';
import { useForm } from 'react-hook-form';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select } from '@/components/ui/select';
import { Button } from '@/components/ui/button';

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
  const { register, handleSubmit, formState: { errors } } = useForm({
    defaultValues: defaultValues || {},
  });

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

            {!readOnly && errors[entry.key] && (
              <span className="text-sm text-red-500">This field is required</span>
            )}
          </div>
        );
      })}

      {!readOnly && <Button type="submit">{submitLabel}</Button>}
    </form>
  );
}
