import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Aufy',
			social: {
				github: 'https://github.com/damianostre/aufy',
			},
			logo: {
				replacesTitle: true,
				dark: './src/assets/logo_dark.svg',
				light: './src/assets/logo_light.svg',
			},
			sidebar: [
				{
					label: 'Manual',
					autogenerate: { directory: 'manual' },
				},
				{
					label: 'Starter Templates',
					autogenerate: { directory: 'starters' },
				},
				{
					label: 'Endpoints',
					collapsed: true,
					autogenerate: { directory: 'endpoints' },
				},
				{
					label: 'FAQ',
					link: '/faq',
				}
			],
		}),
	],
});
