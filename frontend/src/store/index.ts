import {create} from "zustand/react";

interface UIState {
    isCreateMarkerModalOpen: boolean;
    pendingMarker: {lat: number, lng: number} | null;

    setPendingMarker: (lat: number, lng: number) => void;
    openCreateMarkerModal: () => void;
    closeCreateMarkerModal: () => void;
}

export const useUIStore = create<UIState>((set)=>({
    isCreateMarkerModalOpen: false,
    pendingMarker: null,

    setPendingMarker: (lat, lng) => set({pendingMarker:{lat, lng}}),
    openCreateMarkerModal: () => set({isCreateMarkerModalOpen: true}),
    closeCreateMarkerModal: () => set({
        isCreateMarkerModalOpen: false,
        pendingMarker: null,
    })
}))