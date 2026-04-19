import { create } from 'zustand';
import type { Coordinate } from '@/entities/marker/model/types';

export type MapInteractionMode = 'IDLE' | 'SELECT_POINT' | 'DRAW_POLYGON';

interface MapInteractionState {
    mode: MapInteractionMode;
    tempPoints: Coordinate[];
    setMode: (mode: MapInteractionMode) => void;
    addPoint: (p: Coordinate) => void;
    clear: () => void;
}

export const useMapInteractionStore = create<MapInteractionState>((set) => ({
    mode: 'IDLE',
    tempPoints: [],
    setMode: (mode) => set({ mode, tempPoints: [] }),
    addPoint: (p) => set((s) => ({
        tempPoints: s.mode === 'SELECT_POINT' ? [p] : [...s.tempPoints, p]
    })),
    clear: () => set({ mode: 'IDLE', tempPoints: [] })
}));