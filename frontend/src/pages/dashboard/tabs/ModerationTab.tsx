import { ModerationList } from '@/widgets/moderation-list/ui/ModerationList';
import {usePermissions} from "@/features/auth/model/authStore.ts";
export const ModerationTab = () => {
    const { isModerator, isAdmin } = usePermissions();

    if (!isModerator && !isAdmin) return <div>Доступ запрещен</div>;

    return <ModerationList />;
};