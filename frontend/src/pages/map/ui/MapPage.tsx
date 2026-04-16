import { useState } from 'react';
import { MapContainer, TileLayer, Marker as LeafletMarker, Tooltip } from 'react-leaflet';
import MarkerClusterGroup from "react-leaflet-cluster";
import { useQuery } from '@tanstack/react-query';

import { MapHeader } from './MapHeader';
import { MapSidebar } from './MapSidebar';
import { MapBoundsUpdater } from './MapBoundsUpdater';
import { MapController } from './MapController'; // Наш новый файл
import { Modal } from '@/shared/ui/Modal/Modal';

import { LoginForm } from '@/features/auth/ui/LoginForm';
import { RegisterForm } from '@/features/auth/ui/RegisterForm';
import { ProfileModal } from '@/features/auth/ui/ProfileModal';
import { LeaderboardModal } from '@/features/leaderboard/ui/LeaderboardModal';
import { CreateMarkerModal } from '@/features/create-marker/ui/CreateMarkerModal';
import { ViewMarkerModal } from '@/features/view-marker/ui/ViewMarkerModal';
import { createMarkerIcon } from '@/entities/marker/ui/MarkerIcon';

import { useAuthStore } from '@/features/auth/model/authStore';
import { useMarkerStore } from '@/entities/marker/model/store';
import { useFilterStore } from '@/features/filter-markers/model/filterStore';
import { getMarkers } from '@/features/create-marker/model/createMarkerApi';
import { MapClickHandler } from './MapClickHandler';

import styles from './MapPage.module.css';

export const MapPage = () => {
    const { user } = useAuthStore();
    const [modals, setModals] = useState({ login: false, register: false, profile: false, leaderboard: false });

    const openModal = (name: keyof typeof modals) => setModals(prev => ({ ...prev, [name]: true }));
    const closeModal = (name: keyof typeof modals) => setModals(prev => ({ ...prev, [name]: false }));

    const { category, status, bounds } = useFilterStore();

    const { data: markers = [], isFetching } = useQuery({
        queryKey: ['markers', category, status, bounds],
        queryFn: () => getMarkers({ category: category ?? undefined, status: status ?? undefined, ...bounds }),
        placeholderData: (prev) => prev,
        enabled: !!bounds,
    });

    return (
        <div className={styles.app}>
            <MapHeader
                onOpenLeaderboard={() => openModal('leaderboard')}
                onOpenProfile={() => openModal('profile')}
                onOpenLogin={() => openModal('login')}
                onOpenRegister={() => openModal('register')}
            />

            <div className={styles.main}>
                <MapSidebar />
                <div className={styles.mapWrapper}>
                    {isFetching && <div className={styles.loadingOverlay}>Обновление меток...</div>}

                    <MapContainer center={[55.7558, 37.6173]} zoom={13} className={styles.map}>
                        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

                        {/* ЛОГИКА */}
                        <MapController user={user} />
                        <MapBoundsUpdater />
                        <MapClickHandler />

                        <MarkerClusterGroup chunkedLoading>
                            {markers.map((marker) => (
                                <LeafletMarker
                                    key={marker.id}
                                    position={[marker.latitude, marker.longitude]}
                                    icon={createMarkerIcon(marker.category)}
                                    eventHandlers={{
                                        click: () => useMarkerStore.getState().setSelectedMarker(marker),
                                    }}
                                >
                                    <Tooltip direction="top" offset={[0, -20]}>{marker.title}</Tooltip>
                                </LeafletMarker>
                            ))}
                        </MarkerClusterGroup>
                    </MapContainer>
                </div>
            </div>

            {/* Модалки (логика вынесена выше) */}
            <Modal isOpen={modals.login} onClose={() => closeModal('login')}>
                <LoginForm onSuccess={() => closeModal('login')} onSwitch={() => { closeModal('login'); openModal('register'); }} />
            </Modal>
            <Modal isOpen={modals.register} onClose={() => closeModal('register')}>
                <RegisterForm onSuccess={() => closeModal('register')} onSwitch={() => { closeModal('register'); openModal('login'); }} />
            </Modal>
            <ProfileModal isOpen={modals.profile} onClose={() => closeModal('profile')} />
            <LeaderboardModal isOpen={modals.leaderboard} onClose={() => closeModal('leaderboard')} />
            <CreateMarkerModal />
            <ViewMarkerModal />
        </div>
    );
};