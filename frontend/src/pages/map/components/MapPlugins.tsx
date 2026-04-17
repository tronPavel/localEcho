import { useMap, useMapEvents } from 'react-leaflet';
import { useEffect, useState } from 'react';
import { useDebounce } from '@/shared/lib/hooks/useDebounce';
import { useFilterStore } from '@/features/filter-markers/model/filterStore';
import { useCreateMarkerStore } from '@/features/create-marker/model/createMarkerStore';
import { useAuthStore } from '@/features/auth/model/authStore';

/** Логика управления камерой (центрирование) */
export const MapController = () => {
    const map = useMap();
    const user = useAuthStore(s => s.user);

    useEffect(() => {
        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition(
                (pos) => map.flyTo([pos.coords.latitude, pos.coords.longitude], 14),
                () => {
                    if (user?.latitude && user?.longitude) {
                        map.setView([user.latitude, user.longitude], 14);
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

    // 800ms - оптимально для того, чтобы пользователь закончил движение
    const debouncedBounds = useDebounce(localBounds, 600);

    const update = () => {
        const b = map.getBounds();
        const newBounds = {
            minLat: b.getSouth(),
            maxLat: b.getNorth(),
            minLng: b.getWest(),
            maxLng: b.getEast()
        };
        setLocalBounds(newBounds);
    };

    useMapEvents({
        moveend: update,
        zoomend: update
    });

    useEffect(() => {
        if (debouncedBounds) {
            setBounds(debouncedBounds);
        }
    }, [debouncedBounds, setBounds]);

    return null;
};

/** Логика создания маркера по клику */
export const MapClickHandler = () => {
    const { setPendingPosition, openModal } = useCreateMarkerStore();
    const isAuthenticated = useAuthStore(s => s.isAuthenticated);

    useMapEvents({
        click(e) {
            if (isAuthenticated) {
                setPendingPosition({ lat: e.latlng.lat, lng: e.latlng.lng });
                openModal();
            }
        },
    });

    return null;
};