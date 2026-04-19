import { useMap, useMapEvents} from 'react-leaflet';
import {useCallback, useEffect, useState} from 'react';
import { useDebounce } from '@/shared/lib/hooks/useDebounce';
import { useFilterStore } from '@/features/filter-markers/model/filterStore';
import { useAuthStore } from '@/features/auth/model/authStore';
import L from 'leaflet';

export const MapController = () => {
    const map = useMap();
    const user = useAuthStore(s => s.user);

    useEffect(() => {
        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    const { latitude, longitude } = pos.coords;
                    map.flyTo([latitude, longitude], 15);
                    L.circle([latitude, longitude], { radius: 10 }).addTo(map);
                },
                () => {
                    if (user?.latitude && user?.longitude) {
                        map.setView([user.latitude, user.longitude], 14);
                    } else if (user?.districtId) {
                        // Позже добавим flyTo на центр района
                    }
                }
            );
        }
    }, [map, user]);

    return null;
};
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

    // Слушаем события перемещения
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
