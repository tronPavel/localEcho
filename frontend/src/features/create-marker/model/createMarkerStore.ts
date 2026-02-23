import { create } from 'zustand';

interface CreateMarkerState {
    isModalOpen: boolean;
    pendingPosition: { lat: number; lng: number } | null;
    setPendingPosition: (pos: { lat: number; lng: number }) => void;
    openModal: () => void;
    closeModal: () => void;
}

export const useCreateMarkerStore = create<CreateMarkerState>((set) => ({
    isModalOpen: false,
    pendingPosition: null,
    setPendingPosition: (pos) => set({ pendingPosition: pos }),
    openModal: () => set({ isModalOpen: true }),
    closeModal: () => set({ isModalOpen: false, pendingPosition: null }),
}));