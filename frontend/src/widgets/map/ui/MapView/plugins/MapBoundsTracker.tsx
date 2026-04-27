import { useMap, useMapEvents} from 'react-leaflet';
import {useCallback, useEffect, useState} from 'react';
import { useDebounce } from '@/shared/lib/hooks/useDebounce.ts';
import { useFilterStore } from '@/features/filter-markers/model/filterStore.ts';


export const MapBoundsTracker = () => {
    const map = useMap();
    const { setBounds } = useFilterStore();

    const [localBounds, setLocalBounds] = useState<any>(null);

    const debouncedBounds = useDebounce(localBounds, 600);

    const updateInternal = useCallback(() => {
        const b = map.getBounds();
        const next = {
            minLat: b.getSouth(),
            maxLat: b.getNorth(),
            minLng: b.getWest(),
            maxLng: b.getEast()
        };
        setLocalBounds(next);
    }, [map]);

    useMapEvents({
        moveend: updateInternal,
        zoomend: updateInternal,
    });

    useEffect(() => {
        updateInternal();
    }, [updateInternal]);

    useEffect(() => {
        if (debouncedBounds) {
            setBounds(debouncedBounds);
        }
    }, [debouncedBounds, setBounds]);

    return null;
};
