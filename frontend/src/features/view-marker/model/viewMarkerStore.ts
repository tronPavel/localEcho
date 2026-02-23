import { create } from 'zustand';

interface ViewMarkerState {
    isModalOpen: boolean;
    openModal: () => void;
    closeModal: () => void;
}

export const useViewMarkerStore = create<ViewMarkerState>((set) => ({
    isModalOpen: false,
    openModal: () => set({ isModalOpen: true }),
    closeModal: () => set({ isModalOpen: false }),
}));