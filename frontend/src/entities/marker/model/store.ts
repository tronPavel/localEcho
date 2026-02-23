import { create } from 'zustand';
import type { MarkerMapDto } from "./types.ts";

interface MarkerState {
    selectedMarker: MarkerMapDto | null;
    setSelectedMarker: (marker: MarkerMapDto | null) => void;
}

export const useMarkerStore = create<MarkerState>((set) => ({
    selectedMarker: null,
    setSelectedMarker: (marker) => set({ selectedMarker: marker }),
}));