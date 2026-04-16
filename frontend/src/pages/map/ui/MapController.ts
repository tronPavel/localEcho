import { useEffect } from 'react';
import { useMap } from 'react-leaflet';
import type { AuthResponseDto } from '@/features/auth/model/types';

interface MapControllerProps {
    user: AuthResponseDto | null;
}

export const MapController = ({ user }: MapControllerProps) => {
    const map = useMap();

    useEffect(() => {
        const handleInitialPosition = () => {
            // 1. Приоритет: Геолокация браузера (динамически)
            if ("geolocation" in navigator) {
                navigator.geolocation.getCurrentPosition(
                    (pos) => {
                        const { latitude, longitude } = pos.coords;
                        map.flyTo([latitude, longitude], 14, { duration: 1.5 });
                    },
                    (error) => {
                        console.log("Геолокация отклонена или недоступна:", error.message);

                        if (user?.latitude && user?.longitude) {
                            map.setView([user.latitude, user.longitude], 14);
                        }
                        else if (user?.districtId && !user?.latitude) {
                            //TODO
                            // Центрируем на дефолт (в идеале сюда прокинуть координаты района из API)
                            // Если в user прилетает districtName/ID, можно добавить логику поиска центра
                        }
                    }
                );
            }
        };

        handleInitialPosition();
    }, [map, user]);

    return null;
};