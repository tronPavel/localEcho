import { LeaderboardFeature } from '@/features/leaderboard/ui/LeaderboardFeature';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';

export const LeaderboardPage = () => {
    return (
        <RoutedModal title="Рейтинг активистов">
            <LeaderboardFeature />
        </RoutedModal>
    );
};