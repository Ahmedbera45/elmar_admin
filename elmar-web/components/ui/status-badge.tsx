import { Badge } from "@/components/ui/badge";

const STATUS_MAP: Record<number, { label: string; variant: "default" | "secondary" | "destructive" | "outline" | "success" }> = {
  1: { label: "Active", variant: "default" }, // Active - Blue-ish (default)
  2: { label: "Completed", variant: "success" }, // Completed - Green
  3: { label: "Cancelled", variant: "destructive" }, // Cancelled - Red
};

export function StatusBadge({ status }: { status: number }) {
  const config = STATUS_MAP[status] || { label: "Unknown", variant: "outline" };

  // Custom styling for "success" since shadcn/ui might not have it by default
  if (config.variant === "success") {
      return <Badge className="bg-green-500 hover:bg-green-600">{config.label}</Badge>;
  }

  return <Badge variant={config.variant}>{config.label}</Badge>;
}
