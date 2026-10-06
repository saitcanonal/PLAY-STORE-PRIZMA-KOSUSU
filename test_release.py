import unittest
from release import validate

class ReleaseSafety(unittest.TestCase):
    def config(self):
        return dict(STORE_READY=True, PRIVACY_URL='https://prizma.test/privacy',
                    SUPPORT_URL='https://prizma.test/support', SUPPORT_EMAIL='help@prizma.test',
                    ADS_ENABLED=False, LIVE_ADS=False, IAP_ENABLED=False, PRISMA_API='')
    def test_offline_no_ads_requires_no_ad_account(self):
        self.assertEqual(validate(self.config(), 'android'), [])
    def test_demo_ads_cannot_ship(self):
        c = self.config() | dict(ADS_ENABLED=True, LIVE_ADS=True,
            ANDROID_ADMOB_APP_ID='ca-app-pub-3940256099942544~3347511713',
            ANDROID_REWARDED_ID='ca-app-pub-3940256099942544/5224354917')
        self.assertEqual(len(validate(c, 'android')), 2)
    def test_live_flag_and_units_are_required(self):
        c = self.config() | dict(ADS_ENABLED=True)
        self.assertEqual(len(validate(c, 'ios')), 3)
    def test_unverified_services_block_release(self):
        c = self.config() | dict(IAP_ENABLED=True, PRISMA_API='https://api.prizma.test')
        self.assertEqual(len(validate(c, 'ios')), 2)
    def test_private_or_missing_pages_block_release(self):
        c = self.config() | dict(PRIVACY_URL='http://localhost', SUPPORT_URL='', SUPPORT_EMAIL='')
        self.assertEqual(len(validate(c, 'ios')), 3)

if __name__ == '__main__':
    unittest.main()
