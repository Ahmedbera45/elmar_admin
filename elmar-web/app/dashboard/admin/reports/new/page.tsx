'use client';

import { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { customInstance } from '@/lib/api/custom-instance';

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8'];

export default function NewReportPage() {
    const { control, handleSubmit, watch } = useForm({
        defaultValues: {
            processCode: '',
            xAxis: 'Status',
            chartType: 'Bar',
            startDate: '',
            endDate: ''
        }
    });

    const [data, setData] = useState<any[]>([]);
    const [loading, setLoading] = useState(false);

    const onSubmit = async (formData: any) => {
        setLoading(true);
        try {
            const res = await customInstance<any[]>({
                url: '/api/reports/generate',
                method: 'POST',
                data: {
                    ...formData,
                    startDate: formData.startDate || null,
                    endDate: formData.endDate || null
                }
            });
            setData(res);
        } catch (e) {
            console.error(e);
            alert("Failed to generate report");
        } finally {
            setLoading(false);
        }
    };

    const chartType = watch('chartType');

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-6">Custom Report Builder</h1>

            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <div className="bg-white p-4 rounded shadow md:col-span-1 h-fit">
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                        <div className="space-y-2">
                            <Label>Process Code (Optional)</Label>
                            <Controller
                                name="processCode"
                                control={control}
                                render={({ field }) => <Input {...field} placeholder="e.g. LICENSE-01" />}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label>X-Axis (Grouping)</Label>
                            <Controller
                                name="xAxis"
                                control={control}
                                render={({ field }) => (
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                        <SelectTrigger><SelectValue /></SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Status">Request Status</SelectItem>
                                            <SelectItem value="Date">Creation Date</SelectItem>
                                            {/* Ideally we list custom fields here if Process Code selected */}
                                            <SelectItem value="AdaNo">Custom: AdaNo</SelectItem>
                                            <SelectItem value="Mahalle">Custom: Mahalle</SelectItem>
                                        </SelectContent>
                                    </Select>
                                )}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label>Chart Type</Label>
                            <Controller
                                name="chartType"
                                control={control}
                                render={({ field }) => (
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                        <SelectTrigger><SelectValue /></SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Bar">Bar Chart</SelectItem>
                                            <SelectItem value="Pie">Pie Chart</SelectItem>
                                        </SelectContent>
                                    </Select>
                                )}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label>Start Date</Label>
                            <Controller name="startDate" control={control} render={({ field }) => <Input type="date" {...field} />} />
                        </div>
                         <div className="space-y-2">
                            <Label>End Date</Label>
                            <Controller name="endDate" control={control} render={({ field }) => <Input type="date" {...field} />} />
                        </div>

                        <Button type="submit" disabled={loading} className="w-full">
                            {loading ? 'Generating...' : 'Generate Report'}
                        </Button>
                    </form>
                </div>

                <div className="bg-white p-4 rounded shadow md:col-span-3 min-h-[400px] flex items-center justify-center">
                    {data.length === 0 ? (
                        <div className="text-gray-400">No data generated yet.</div>
                    ) : (
                        <ResponsiveContainer width="100%" height={400}>
                            {chartType === 'Bar' ? (
                                <BarChart data={data}>
                                    <CartesianGrid strokeDasharray="3 3" />
                                    <XAxis dataKey="label" />
                                    <YAxis />
                                    <Tooltip />
                                    <Legend />
                                    <Bar dataKey="value" fill="#8884d8" name="Count" />
                                </BarChart>
                            ) : (
                                <PieChart>
                                    <Pie
                                        data={data}
                                        cx="50%"
                                        cy="50%"
                                        labelLine={false}
                                        label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                                        outerRadius={150}
                                        fill="#8884d8"
                                        dataKey="value"
                                    >
                                        {data.map((entry, index) => (
                                            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                                        ))}
                                    </Pie>
                                    <Tooltip />
                                </PieChart>
                            )}
                        </ResponsiveContainer>
                    )}
                </div>
            </div>
        </div>
    );
}
