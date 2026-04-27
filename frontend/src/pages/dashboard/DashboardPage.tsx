import { Outlet, NavLink } from 'react-router-dom';
import { usePermissions } from '@/features/auth/model/authStore';
import { classNames } from '@/shared/lib/utils/classNames';
import styles from './DashboardPage.module.css';

export const DashboardPage = () => {
    const { isAdmin, isModerator, isOfficial } = usePermissions();

    return (
        <div className={styles.container}>
            <aside className={styles.sidebar}>
                <div className={styles.sidebarHeader}>
                    <h3>Панель управления</h3>
                    <div className={styles.accentLine} />
                </div>

                <nav className={styles.nav}>
                    {(isAdmin || isModerator) && (
                        <NavLink to="reports" className={({isActive}) => classNames(styles.link, isActive && styles.active)}>
                            🚩 Жалобы жителей
                        </NavLink>
                    )}

                    {(isAdmin || isOfficial) && (
                        <NavLink to="tasks" className={({isActive}) => classNames(styles.link, isActive && styles.active)}>
                            📋 Очередь задач
                        </NavLink>
                    )}

                    {isAdmin && (
                        <>
                            <NavLink to="users" className={({isActive}) => classNames(styles.link, isActive && styles.active)}>
                                👥 Пользователи
                            </NavLink>
                            <NavLink to="districts" className={({isActive}) => classNames(styles.link, isActive && styles.active)}>
                                🗺 Границы районов
                            </NavLink>
                        </>
                    )}
                </nav>
            </aside>

            <main className={styles.content}>
                <Outlet />
            </main>
        </div>
    );
};