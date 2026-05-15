import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface CityState {
    currentCityId: string | null;
    currentCityName: string;
    mapFocus: [number, number] | null;
    setCity: (id: string | null, name: string, coords?: [number, number] | null) => void;
}

export const useCityStore = create<CityState>()(
    persist(
        (set) => ({
            currentCityId: null,
            currentCityName: 'Все города',
            mapFocus: null,
            setCity: (id, name, coords = null) => set({
                currentCityId: id,
                currentCityName: name,
                mapFocus: coords
            }),
        }),
        { name: 'city-storage' }
    )
);