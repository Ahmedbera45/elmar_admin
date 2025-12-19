'use client';

import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Select } from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Trash, Plus } from 'lucide-react';

interface ConditionRow {
  field: string;
  operator: string;
  value: string;
  logic: 'AND' | 'OR';
}

interface ConditionBuilderProps {
  fields: { key: string; title: string; type: string }[];
  value: string; // The resulting RuleExpression
  onChange: (value: string) => void;
}

export function ConditionBuilder({ fields, value, onChange }: ConditionBuilderProps) {
  const [rows, setRows] = useState<ConditionRow[]>([]);
  const [isRawMode, setIsRawMode] = useState(false);
  const [rawValue, setRawValue] = useState(value);

  // Attempt to parse simple expressions or start empty
  useEffect(() => {
    setRawValue(value);
    // TODO: A real parser would be needed here to convert string back to rows.
    // For now, if value exists and rows are empty, we might default to Raw Mode or just clear rows.
    if (value && rows.length === 0) {
        setIsRawMode(true);
    }
  }, [value]);

  const updateExpression = (newRows: ConditionRow[]) => {
    let expr = "";
    newRows.forEach((row, index) => {
      let val = row.value;
      // Quote strings
      const fieldDef = fields.find(f => f.key === row.field);
      if (fieldDef?.type === 'Text' || fieldDef?.type === 'Select' || isNaN(Number(val))) {
          if (!val.startsWith('"') && !val.endsWith('"')) {
            val = `"${val}"`;
          }
      }

      const part = `${row.field} ${row.operator} ${val}`;
      if (index === 0) {
        expr = part;
      } else {
        expr += ` ${row.logic} ${part}`; // Note: System.Linq.Dynamic.Core uses 'and', 'or', '&&', '||'
        // Let's use 'and', 'or' for readability if supported, or '&&', '||'
        // C# Dynamic Linq supports 'and', 'or'.
      }
    });
    // Update parent
    onChange(expr);
  };

  const addRow = () => {
    const newRows = [...rows, { field: fields[0]?.key || '', operator: '==', value: '', logic: 'AND' }];
    setRows(newRows);
    updateExpression(newRows);
  };

  const removeRow = (index: number) => {
    const newRows = rows.filter((_, i) => i !== index);
    setRows(newRows);
    updateExpression(newRows);
  };

  const updateRow = (index: number, key: keyof ConditionRow, val: string) => {
    const newRows = [...rows];
    newRows[index] = { ...newRows[index], [key]: val };
    setRows(newRows);
    updateExpression(newRows);
  };

  const operators = [
    { label: 'Equals (==)', value: '==' },
    { label: 'Not Equals (!=)', value: '!=' },
    { label: 'Greater (>)', value: '>' },
    { label: 'Less (<)', value: '<' },
    { label: 'Contains', value: '.Contains' }, // Special handling might be needed
  ];

  if (isRawMode) {
    return (
      <div className="space-y-2">
         <div className="flex justify-between">
            <label className="text-sm font-medium">Rule Expression (C# Dynamic LINQ)</label>
            <Button variant="ghost" size="sm" onClick={() => setIsRawMode(false)}>Switch to Builder</Button>
         </div>
         <Input value={rawValue} onChange={e => { setRawValue(e.target.value); onChange(e.target.value); }} />
         <p className="text-xs text-gray-500">Example: Amount &gt; 5000 and Status == "Pending"</p>
      </div>
    );
  }

  return (
    <div className="space-y-2 border rounded p-3 bg-gray-50">
      <div className="flex justify-between items-center mb-2">
        <label className="text-sm font-medium">Conditions</label>
        <Button variant="ghost" size="sm" onClick={() => setIsRawMode(true)}>Switch to Raw Code</Button>
      </div>

      {rows.map((row, index) => (
        <div key={index} className="flex gap-2 items-center">
           {index > 0 && (
             <select
               className="border rounded px-2 py-1 text-sm w-20"
               value={row.logic}
               onChange={e => updateRow(index, 'logic', e.target.value as any)}
             >
               <option value="AND">AND</option>
               <option value="OR">OR</option>
             </select>
           )}

           <select
             className="border rounded px-2 py-1 text-sm flex-1"
             value={row.field}
             onChange={e => updateRow(index, 'field', e.target.value)}
           >
             {fields.map(f => <option key={f.key} value={f.key}>{f.title}</option>)}
           </select>

           <select
             className="border rounded px-2 py-1 text-sm w-32"
             value={row.operator}
             onChange={e => updateRow(index, 'operator', e.target.value)}
           >
             {operators.map(op => <option key={op.value} value={op.value}>{op.label}</option>)}
           </select>

           <Input
             className="flex-1 h-8"
             value={row.value}
             onChange={e => updateRow(index, 'value', e.target.value)}
             placeholder="Value"
           />

           <Button variant="ghost" size="icon" onClick={() => removeRow(index)}>
             <Trash className="h-4 w-4 text-red-500" />
           </Button>
        </div>
      ))}

      <Button variant="outline" size="sm" onClick={addRow} className="mt-2">
        <Plus className="h-3 w-3 mr-1" /> Add Condition
      </Button>
    </div>
  );
}
