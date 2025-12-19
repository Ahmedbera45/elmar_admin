"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { ProcessRequestFilter } from "@/lib/api/generated";
import { X } from "lucide-react";

interface RequestFiltersProps {
  onFilterChange: (filter: ProcessRequestFilter) => void;
}

export function RequestFilters({ onFilterChange }: RequestFiltersProps) {
  const [status, setStatus] = useState<string>("all");
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");

  const handleApply = () => {
    const filter: ProcessRequestFilter = {};
    if (status !== "all") filter.status = parseInt(status);
    if (startDate) filter.startDate = startDate;
    if (endDate) filter.endDate = endDate;
    onFilterChange(filter);
  };

  const handleClear = () => {
    setStatus("all");
    setStartDate("");
    setEndDate("");
    onFilterChange({});
  };

  return (
    <div className="bg-white p-4 rounded-lg border shadow-sm mb-4 space-y-4 md:space-y-0 md:flex md:items-end md:gap-4">
      <div className="space-y-2">
        <Label>Status</Label>
        <Select value={status} onValueChange={setStatus}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="All Statuses" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All</SelectItem>
            <SelectItem value="1">Active</SelectItem>
            <SelectItem value="2">Completed</SelectItem>
            <SelectItem value="3">Cancelled</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-2">
        <Label>Start Date</Label>
        <Input
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
          className="w-[180px]"
        />
      </div>

      <div className="space-y-2">
        <Label>End Date</Label>
        <Input
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
          className="w-[180px]"
        />
      </div>

      <div className="flex gap-2">
        <Button onClick={handleApply}>Apply</Button>
        <Button variant="outline" onClick={handleClear} size="icon" title="Clear Filters">
            <X className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
