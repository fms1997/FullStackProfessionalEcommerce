import { type ReactNode, useMemo, useState } from "react";

type VirtualizedListProps<T> = {
  items: T[];
  height: number;
  itemHeight: number;
  overscan?: number;
  renderItem: (item: T, index: number) => ReactNode;
};

export function VirtualizedList<T>({
  items,
  height,
  itemHeight,
  overscan = 4,
  renderItem,
}: VirtualizedListProps<T>) {
  const [scrollTop, setScrollTop] = useState(0);

  const { start, end } = useMemo(() => {
    const visibleCount = Math.ceil(height / itemHeight);
    const calculatedStart = Math.max(0, Math.floor(scrollTop / itemHeight) - overscan);
    const calculatedEnd = Math.min(
      items.length,
      calculatedStart + visibleCount + overscan * 2,
    );

    return { start: calculatedStart, end: calculatedEnd };
  }, [height, itemHeight, items.length, overscan, scrollTop]);

  const offsetY = start * itemHeight;
  const totalHeight = items.length * itemHeight;
  const visibleItems = items.slice(start, end);

  return (
    <div
      style={{ height, overflowY: "auto" }}
      onScroll={(e) => setScrollTop(e.currentTarget.scrollTop)}
    >
      <div style={{ height: totalHeight, position: "relative" }}>
        <div style={{ transform: `translateY(${offsetY}px)` }}>
          {visibleItems.map((item, idx) => (
            <div key={start + idx} style={{ height: itemHeight }}>
              {renderItem(item, start + idx)}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
