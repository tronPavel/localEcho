import { useMapEvents } from 'react-leaflet';
import { useEffect, useState } from 'react';
import { useFilterStore } from '../../../features/filter-markers/model/filterStore';
import { useDebounce } from '../../../shared/lib/hooks/useDebounce';

export const MapBoundsUpdater = () => {
    // @ts-ignore
    const { setBounds } = useFilterStore();
    const [localBounds, setLocalBounds] = useState<any>(null);

    const map = useMapEvents({
        moveend: () => {
            const b = map.getBounds();
            setLocalBounds({
                minLat: b.getSouth(),
                maxLat: b.getNorth(),
                minLng: b.getWest(),
                maxLng: b.getEast(),
            });
        },
        load: () => {
            const b = map.getBounds();
            setLocalBounds({
                minLat: b.getSouth(),
                maxLat: b.getNorth(),
                minLng: b.getWest(),
                maxLng: b.getEast(),
            });
        }
    });

    const debouncedBounds = useDebounce(localBounds, 500);

    useEffect(() => {
        if (debouncedBounds) {
            setBounds(debouncedBounds);
        }
    }, [debouncedBounds, setBounds]);

    useEffect(() => {
        if (!localBounds && map) {
            const b = map.getBounds();
            setLocalBounds({
                minLat: b.getSouth(),
                maxLat: b.getNorth(),
                minLng: b.getWest(),
                maxLng: b.getEast(),
            });
        }
    }, [map]);

    return null;
};