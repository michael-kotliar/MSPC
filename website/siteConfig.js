/**
 * Copyright (c) 2017-present, Facebook, Inc.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

// See https://docusaurus.io/docs/site-config for all the possible
// site configuration options.


const siteConfig = {
  title: 'MSPC', // Title for your website.
  tagline: 'Using combined evidence from replicates to evaluate ChIP-seq peaks',
  url: 'https://genometric.github.io', // Your website URL
  baseUrl: '/MSPC/', // Base URL for your project */
  // For github.io type URLs, you would set the url and baseUrl like:
  //   url: 'https://facebook.github.io',
  //   baseUrl: '/test-site/',

  // Used for publishing and more
  projectName: 'MSPC',
  organizationName: 'Genometric',
  // For top-level user or org sites, the organization is still the same.
  // e.g., for the https://JoelMarcey.github.io site, it would be set like...
  //   organizationName: 'JoelMarcey'

  // For no header links in the top nav bar -> headerLinks: [],
  headerLinks: [
    {label: 'Documentation', doc: 'welcome'},
    {label: 'Source Code', href: 'https://github.com/Genometric/MSPC/'},
    {label: 'Download', href: 'https://github.com/Genometric/MSPC/releases'},
    {label: 'Questions', href: 'https://github.com/Genometric/MSPC/issues'},
    {label: 'Publications', page: 'publications'},
  ],

  /* path to images for header/footer */
  headerIcon: 'img/logo.svg',
  footerIcon: 'img/logo.svg',
  favicon: 'img/logo.png',

  /* Colors for website */
  colors: {
    primaryColor: '#13001e', // '#321730',
    secondaryColor: '#96af4c',
  },

  /* Custom fonts for website */
//  fonts: {
//    myFont: [
//      "Times New Roman",
//      "Serif"
//    ],
//    myOtherFont: [
//      "-apple-system",
//      "system-ui"
//    ]
//  },

  // This copyright info is used in /core/Footer.js and blog RSS/Atom feeds.
  copyright: `Copyright © ${new Date().getFullYear()} Genometric`,

  highlight: {
    // Highlight.js theme to use for syntax highlighting in code blocks.
//    theme: 'dracula',
//    theme: 'sunburst',
    theme: 'vs2015',
  },

  // Add custom scripts here that would be placed in <script> tags.
  scripts: ['https://buttons.github.io/buttons.js',
     {
         src: 'https://d1bxh8uas1mnw7.cloudfront.net/assets/embed.js',
         async: true
     },
     {
         src: 'https://badge.dimensions.ai/badge.js',
         async: true
     }
  ],

  // On page navigation for the current documentation page.
  onPageNav: 'separate',
  // No .html extensions for paths.
  cleanUrl: true,

  // You may provide arbitrary config keys to be used as needed by your
  // template. For example, if you need your repo's URL...
  //   repoUrl: 'https://github.com/facebook/test-site',

  noIndex: false,

  enableUpdateTime: true,

  gaTrackingId: 'UA-100863841-2',
};

module.exports = siteConfig;
