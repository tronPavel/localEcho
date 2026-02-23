import { create } from 'zustand';

interface FilterState {
    category: string | null;
    status: string | null;
    bounds: { minLat: number; maxLat: number; minLng: number; maxLng: number } | null;

    setCategory: (cat: string | null) => void;
    setStatus: (stat: string | null) => void;
    setBounds: (bounds: FilterState['bounds']) => void;
}

export const useFilterStore = create<FilterState>((set) => ({
    category: null,
    status: null,
    bounds: null,
    setCategory: (cat) => set({ category: cat }),
    setStatus: (stat) => set({ status: stat }),
    setBounds: (bounds) => set({ bounds }),
}));