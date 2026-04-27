import { useMapInteractionStore } from '@/features/map-control/model/interactionStore.ts';
import { usePermissions } from '@/features/auth/model/authStore.ts';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/ui/Button/Button.tsx';
import styles from './MapToolbar.module.css';

export const MapToolbar = () => {
    const { mode, setMode, tempPoints, clear } = useMapInteractionStore();
    const { isAuthenticated, canDrawPolygons } = usePermissions();
    const navigate = useNavigate();

    if (!isAuthenticated) return null;

    if (mode === 'IDLE') {
        return (
            <div className={styles.container}>
                <Button onClick={() => setMode('SELECT_POINT')}>➕ Новая метка</Button>
            </div>
        );
    }

    return (
        <div className={styles.activeContainer}>
            <p>{mode === 'SELECT_POINT' ? 'Кликните на карту' : 'Отметьте область'}</p>

            <div className={styles.group}>
                {canDrawPolygons && mode === 'SELECT_POINT' && (
                    <Button variant="outline" size="small" onClick={() => setMode('DRAW_POLYGON')}>
                        📐 Рисовать зону
                    </Button>
                )}

                {(mode === 'SELECT_POINT' && tempPoints.length === 1) ||
                (mode === 'DRAW_POLYGON' && tempPoints.length >= 3) ? (
                    <Button size="small" onClick={() => navigate('/create-marker')}>
                        Продолжить ({tempPoints.length})
                    </Button>
                ) : null}

                <Button variant="secondary" size="small" onClick={clear}>Отмена</Button>
            </div>
        </div>
    );
};