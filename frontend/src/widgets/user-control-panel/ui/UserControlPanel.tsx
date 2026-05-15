import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { adminUserApi } from '@/features/admin-user-management/api/adminUserApi';
import { Input } from '@/shared/ui/Input/Input';
import { Button } from '@/shared/ui/Button/Button';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { useDebounce } from '@/shared/lib/hooks/useDebounce';
import { toast } from 'sonner';
import styles from './UserControlPanel.module.css';

export const UserControlPanel = () => {
    const [search, setSearch] = useState('');
    const debouncedSearch = useDebounce(search, 500);
    const queryClient = useQueryClient();

    const { data: users = [], isFetching } = useQuery({
        queryKey: ['admin-search', debouncedSearch],
        queryFn: () => adminUserApi.search(debouncedSearch),
        enabled: debouncedSearch.length > 2
    });

    const addRoleMutation = useMutation({
        mutationFn: ({ userId, role }: { userId: string, role: string }) =>
            adminUserApi.changeRole(userId, role),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-search'] });
            toast.success("Роль успешно добавлена");
        }
    });

    const removeRoleMutation = useMutation({
        mutationFn: ({ userId, role }: { userId: string, role: string }) =>
            adminUserApi.removeRole(userId, role),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin-search'] });
            toast.info("Роль отозвана");
        }
    });

    return (
        <div className={styles.container}>
            <div className={styles.searchBar}>
                <Input
                    placeholder="Поиск по Email или Имени (мин. 3 символа)..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                {isFetching && <span className={styles.loader}>🔍</span>}
            </div>

            <div className={styles.list}>
                {users.map(u => {
                    const hasOfficial = u.roles.includes('Official');
                    const hasModerator = u.roles.includes('Moderator');

                    return (
                        <div key={u.id} className={styles.userCard}>
                            <UserAvatar user={u} size="medium" />
                            <div className={styles.info}>
                                <span className={styles.name}>{u.name}</span>
                                <span className={styles.email}>{u.email}</span>
                                <div className={styles.roles}>
                                    {u.roles.map(r => (
                                        <span key={r} className={styles.roleBadge}>
                                            {r}
                                            {/* Не даем удалять базовую роль User для безопасности */}
                                            {r !== 'User' && (
                                                <button
                                                    className={styles.removeRoleBtn}
                                                    onClick={() => removeRoleMutation.mutate({ userId: u.id, role: r })}
                                                    title="Удалить роль"
                                                >
                                                    ×
                                                </button>
                                            )}
                                        </span>
                                    ))}
                                </div>
                            </div>
                            <div className={styles.actions}>
                                {!hasOfficial && (
                                    <Button
                                        variant="outline"
                                        size="small"
                                        onClick={() => addRoleMutation.mutate({ userId: u.id, role: 'Official' })}
                                    >
                                        + Official
                                    </Button>
                                )}
                                {!hasModerator && (
                                    <Button
                                        variant="outline"
                                        size="small"
                                        onClick={() => addRoleMutation.mutate({ userId: u.id, role: 'Moderator' })}
                                    >
                                        + Moderator
                                    </Button>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};